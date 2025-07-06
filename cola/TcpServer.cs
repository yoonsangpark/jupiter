using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace cola
{
    public class TcpServer
    {
        private TcpListener tcpListener;
        private List<TcpClient> clients;
        private List<Thread> clientThreads;
        private bool isRunning;
        private int port;

        private readonly object lockObject = new object();

        public TcpServer(int port)
        {
            this.port = port;
            this.clients = new List<TcpClient>();
            this.clientThreads = new List<Thread>();
        }

        public void Start()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                isRunning = true;

                SLog.log(Level.INFO, $"TCP 서버가 포트 {port}에서 시작되었습니다.");

                // 클라이언트 연결을 받는 메인 스레드
                Thread acceptThread = new Thread(AcceptClients);
                acceptThread.Start();

                SLog.log(Level.INFO, "TCP 서버가 클라이언트 연결을 대기 중입니다...");
            }
            catch (Exception ex)
            {
                SLog.log(Level.ERROR, $"TCP 서버 시작 오류: {ex.Message}");
            }
        }

        private void AcceptClients()
        {
            while (isRunning)
            {
                try
                {
                    TcpClient client = tcpListener.AcceptTcpClient();
                    SLog.log(Level.INFO, $"새 클라이언트 연결: {((IPEndPoint)client.Client.RemoteEndPoint).ToString()}");

                    // 클라이언트를 리스트에 추가
                    lock (lockObject)
                    {
                        clients.Add(client);
                    }

                    // 각 클라이언트를 위한 별도 스레드 생성
                    Thread clientThread = new Thread(() => HandleClient(client));
                    clientThread.Start();
                    clientThreads.Add(clientThread);
                }
                catch (Exception ex)
                {
                    if (isRunning)
                    {
                        SLog.log(Level.ERROR, $"클라이언트 연결 수락 오류: {ex.Message}");
                    }
                }
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            IPEndPoint clientEndPoint = (IPEndPoint)client.Client.RemoteEndPoint;

            try
            {
                while (isRunning && client.Connected)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // 클라이언트가 연결을 끊음
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    SLog.log(Level.INFO, $"TCP 메시지 수신: {clientEndPoint} -> {message}");

                    // 에코 응답
                    string response = $"서버 응답: {message}";
                    byte[] responseData = Encoding.UTF8.GetBytes(response);
                    stream.Write(responseData, 0, responseData.Length);
                    SLog.log(Level.INFO, $"TCP 응답 전송: {clientEndPoint} <- {response}");

                    // 모든 클라이언트에게 브로드캐스트 (선택사항)
                    BroadcastMessage($"브로드캐스트: {clientEndPoint}가 메시지를 보냄 - {message}", client);
                }
            }
            catch (Exception ex)
            {
                SLog.log(Level.ERROR, $"클라이언트 처리 오류 ({clientEndPoint}): {ex.Message}");
            }
            finally
            {
                // 클라이언트 정리
                lock (lockObject)
                {
                    clients.Remove(client);
                }
                client.Close();
                SLog.log(Level.INFO, $"클라이언트 연결 종료: {clientEndPoint}");
            }
        }

        public void BroadcastMessage(string message, TcpClient excludeClient = null)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            List<TcpClient> clientsToRemove = new List<TcpClient>();

            lock (lockObject)
            {
                foreach (TcpClient client in clients)
                {
                    if (client == excludeClient || !client.Connected)
                    {
                        if (!client.Connected)
                        {
                            clientsToRemove.Add(client);
                        }
                        continue;
                    }

                    try
                    {
                        NetworkStream stream = client.GetStream();
                        stream.Write(data, 0, data.Length);
                        SLog.log(Level.INFO, $"브로드캐스트 전송: {((IPEndPoint)client.Client.RemoteEndPoint).ToString()} <- {message}");
                    }
                    catch (Exception ex)
                    {
                        SLog.log(Level.ERROR, $"브로드캐스트 전송 오류: {ex.Message}");
                        clientsToRemove.Add(client);
                    }
                }

                // 연결이 끊어진 클라이언트들 제거
                foreach (TcpClient client in clientsToRemove)
                {
                    clients.Remove(client);
                }
            }
        }

        public void SendMessageToClient(string message, TcpClient targetClient)
        {
            if (targetClient == null || !targetClient.Connected)
            {
                SLog.log(Level.WARN, "대상 클라이언트가 연결되지 않았습니다.");
                return;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                NetworkStream stream = targetClient.GetStream();
                stream.Write(data, 0, data.Length);
                SLog.log(Level.INFO, $"개별 메시지 전송: {((IPEndPoint)targetClient.Client.RemoteEndPoint).ToString()} <- {message}");
            }
            catch (Exception ex)
            {
                SLog.log(Level.ERROR, $"개별 메시지 전송 오류: {ex.Message}");
            }
        }

        public int GetConnectedClientCount()
        {
            lock (lockObject)
            {
                return clients.Count;
            }
        }

        public List<IPEndPoint> GetConnectedClients()
        {
            List<IPEndPoint> clientEndPoints = new List<IPEndPoint>();
            lock (lockObject)
            {
                foreach (TcpClient client in clients)
                {
                    if (client.Connected)
                    {
                        clientEndPoints.Add((IPEndPoint)client.Client.RemoteEndPoint);
                    }
                }
            }
            return clientEndPoints;
        }

        public void Stop()
        {
            isRunning = false;

            // 모든 클라이언트 연결 종료
            lock (lockObject)
            {
                foreach (TcpClient client in clients)
                {
                    try
                    {
                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        SLog.log(Level.ERROR, $"클라이언트 종료 오류: {ex.Message}");
                    }
                }
                clients.Clear();
            }

            // 모든 클라이언트 스레드 종료 대기
            foreach (Thread thread in clientThreads)
            {
                if (thread.IsAlive)
                {
                    thread.Join(1000); // 1초 대기
                }
            }
            clientThreads.Clear();

            // 리스너 종료
            tcpListener?.Stop();
            SLog.log(Level.INFO, "TCP 서버가 중지되었습니다.");
        }

        public bool IsRunning => isRunning;
    }
} 