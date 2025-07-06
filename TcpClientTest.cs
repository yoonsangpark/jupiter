using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace cola
{
    public class TcpClientTest
    {
        private TcpClient tcpClient;
        private NetworkStream stream;
        private Thread receiveThread;
        private bool isConnected;
        private Action<string> logCallback;

        public TcpClientTest(Action<string> logCallback)
        {
            this.logCallback = logCallback;
        }

        public bool Connect(string serverIp, int serverPort)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(serverIp, serverPort);
                stream = tcpClient.GetStream();
                isConnected = true;

                logCallback?.Invoke($"TCP 클라이언트가 {serverIp}:{serverPort}에 연결되었습니다.");

                // 응답을 받는 스레드 시작
                receiveThread = new Thread(ReceiveMessages);
                receiveThread.Start();

                return true;
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"TCP 클라이언트 연결 오류: {ex.Message}");
                return false;
            }
        }

        public bool SendMessage(string message)
        {
            if (!isConnected || stream == null)
            {
                logCallback?.Invoke("TCP 클라이언트가 연결되지 않았습니다.");
                return false;
            }

            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                stream.Write(data, 0, data.Length);
                logCallback?.Invoke($"TCP 메시지 전송: {message}");
                return true;
            }
            catch (Exception ex)
            {
                logCallback?.Invoke($"TCP 메시지 전송 오류: {ex.Message}");
                return false;
            }
        }

        private void ReceiveMessages()
        {
            byte[] buffer = new byte[1024];

            while (isConnected && tcpClient.Connected)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        // 서버가 연결을 끊음
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    logCallback?.Invoke($"TCP 응답 수신: {message}");
                }
                catch (Exception ex)
                {
                    if (isConnected)
                    {
                        logCallback?.Invoke($"TCP 응답 수신 오류: {ex.Message}");
                    }
                    break;
                }
            }

            isConnected = false;
            logCallback?.Invoke("TCP 클라이언트 연결이 종료되었습니다.");
        }

        public void Disconnect()
        {
            isConnected = false;

            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Join(1000); // 1초 대기
            }

            stream?.Close();
            tcpClient?.Close();

            logCallback?.Invoke("TCP 클라이언트가 종료되었습니다.");
        }

        public bool IsConnected => isConnected && tcpClient?.Connected == true;
    }
} 