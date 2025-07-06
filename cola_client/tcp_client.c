#define _CRT_SECURE_NO_WARNINGS
#define _WINSOCK_DEPRECATED_NO_WARNINGS

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <windows.h>

#pragma comment(lib, "ws2_32.lib")

#define BUFFER_SIZE 1024
#define DEFAULT_PORT 8889
#define DEFAULT_IP "127.0.0.1"

typedef struct tcp_client {
    SOCKET sockfd;
    struct sockaddr_in server_addr;
    char buffer[BUFFER_SIZE];
} tcp_client_t;

// TCP 클라이언트 초기화
int tcp_client_init(tcp_client_t *client, const char *ip, int port) {
    // Winsock 초기화
    WSADATA wsaData;
    if (WSAStartup(MAKEWORD(2, 2), &wsaData) != 0) {
        printf("Winsock 초기화 실패\n");
        return -1;
    }
    
    // 소켓 생성
    client->sockfd = socket(AF_INET, SOCK_STREAM, 0);
    if (client->sockfd == INVALID_SOCKET) {
        printf("소켓 생성 실패: %d\n", WSAGetLastError());
        WSACleanup();
        return -1;
    }

    // 서버 주소 설정
    memset(&client->server_addr, 0, sizeof(client->server_addr));
    client->server_addr.sin_family = AF_INET;
    client->server_addr.sin_port = htons(port);
    
    if (inet_pton(AF_INET, ip, &client->server_addr.sin_addr) <= 0) {
        printf("IP 주소 변환 실패\n");
        closesocket(client->sockfd);
        WSACleanup();
        return -1;
    }

    printf("TCP Client Init. Done\n");
    return 0;
}

// 서버에 연결
int tcp_client_connect(tcp_client_t *client) {
    if (connect(client->sockfd, (struct sockaddr *)&client->server_addr, 
                sizeof(client->server_addr)) == SOCKET_ERROR) {
        printf("서버 연결 실패: %d\n", WSAGetLastError());
        return -1;
    }
    
    printf("Connected %s:%d\n", 
           inet_ntoa(client->server_addr.sin_addr), 
           ntohs(client->server_addr.sin_port));
    return 0;
}

// 메시지 전송
int tcp_client_send(tcp_client_t *client, const char *message) {
    int bytes_sent = send(client->sockfd, message, (int)strlen(message), 0);
    if (bytes_sent == SOCKET_ERROR) {
        printf("메시지 전송 실패: %d\n", WSAGetLastError());
        return -1;
    }
    
    printf("Tx : %s (%d bytes)\n", message, bytes_sent);
    return bytes_sent;
}

// 메시지 수신
int tcp_client_receive(tcp_client_t *client) {
    memset(client->buffer, 0, BUFFER_SIZE);
    int bytes_received = recv(client->sockfd, client->buffer, BUFFER_SIZE - 1, 0);
    
    if (bytes_received == SOCKET_ERROR) {
        printf("메시지 수신 실패: %d\n", WSAGetLastError());
        return -1;
    } else if (bytes_received == 0) {
        printf("서버가 연결을 종료했습니다.\n");
        return 0;
    }
    
    client->buffer[bytes_received] = '\0';
    printf("Rx: %s (%d bytes)\n", client->buffer, bytes_received);
    return bytes_received;
}

// 클라이언트 종료
void tcp_client_close(tcp_client_t *client) {
    if (client->sockfd != INVALID_SOCKET) {
        closesocket(client->sockfd);
        printf("TCP 클라이언트 연결 종료\n");
    }
    WSACleanup();
}

// 대화형 모드
void tcp_client_interactive(tcp_client_t *client) {
    char input[BUFFER_SIZE];
    
    printf("\n=== TCP Client Interactive ===\n");
    printf("Input Msg (Exit : 'quit' or 'exit')\n");
    
    while (1) {
        printf("> ");
        if (fgets(input, BUFFER_SIZE, stdin) == NULL) {
            break;
        }
        
        // 개행 문자 제거
        input[strcspn(input, "\n")] = 0;
        
        // 종료 조건 확인
        if (strcmp(input, "quit") == 0 || strcmp(input, "exit") == 0) {
            printf("클라이언트를 종료합니다.\n");
            break;
        }
        
        // 빈 메시지 무시
        if (strlen(input) == 0) {
            continue;
        }
        
        // 메시지 전송
        if (tcp_client_send(client, input) < 0) {
            break;
        }
        
        // 응답 수신
        if (tcp_client_receive(client) <= 0) {
            break;
        }
    }
}

void tcp_client_test(tcp_client_t *client, int test_count) {
    char test_message[BUFFER_SIZE];
    
    printf("\n=== TCP Test ===\n");
    printf("Count: %d\n", test_count);
    
    for (int i = 1; i <= test_count; i++) {
        snprintf(test_message, BUFFER_SIZE, "MSG #%d", i);
        
        printf("\n--- TEST  %d/%d ---\n", i, test_count);
        
        // Tx
        if (tcp_client_send(client, test_message) < 0) {
            printf("Stop : Tx Fail\n");
            break;
        }
        
        // Rx
        if (tcp_client_receive(client) <= 0) {
            printf("Stop : Rx Fail\n");
            break;
        }
        
        Sleep(1000);
    }
    
    printf("\nDone\n");
}

int main(int argc, char *argv[]) {
    tcp_client_t client;
    char server_ip[16] = DEFAULT_IP;
    int server_port = DEFAULT_PORT;
    int test_mode = 0;
    int test_count = 1;   
      
    printf("TCP Client Start...\n");
    printf("Server : %s:%d\n", server_ip, server_port);
    
    // 클라이언트 초기화
    if (tcp_client_init(&client, server_ip, server_port) < 0) {
        return 1;
    }
    
    // 서버 연결
    if (tcp_client_connect(&client) < 0) {
        tcp_client_close(&client);
        return 1;
    }
    
    // 모드에 따른 실행
    if (test_mode) {
        tcp_client_test(&client, test_count);
    } else {
        tcp_client_interactive(&client);
    }
    
    // 클라이언트 종료
    tcp_client_close(&client);
    
    return 0;
} 