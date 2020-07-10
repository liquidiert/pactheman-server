#include "connector.hpp"

void Connector::init() {
    sock_max = sock1 = create_socket(AF_INET, SOCK_STREAM, 0);
    bind_socket(&sock1, INADDR_ANY, 15000);
    listen_socket(&sock1);

    for(socket_places = 0; socket_places < FD_SETSIZE; socket_places++)
        client_sock[socket_places] = -1;
    FD_ZERO(&overall_sock);
    FD_SET(sock1, &overall_sock);
}

///
/// creates socket todo: add params and return val
///
int Connector::create_socket(int af, int type, int protocol) {
    socket_t sock;
    const int optVal = 1;
    sock = socket(af, type, protocol);

    if(sock < 0) Logger::getInstance().error_n_out("couldn't create socket");

    setsockopt(sock, SOL_SOCKET, SO_REUSEADDR, &optVal, sizeof(int));

    return sock;
}

/// bind to server port (default is 5387) todo: add params and return val
void Connector::bind_socket(socket_t *sock, unsigned long adress,
                            unsigned short port) {
    struct sockaddr_in server;
    memset(&server, 0, sizeof(server));
    server.sin_family = AF_INET;
    server.sin_addr.s_addr = htonl(adress);
    server.sin_port = htons(port);
    if(bind(*sock, (struct sockaddr *)&server, sizeof(server)) < 0)
        Logger::getInstance().error_n_out("can't bind to port");
}

void Connector::listen_socket(socket_t *sock) {
    if(listen(*sock, 5) == -1)
        Logger::getInstance().error_n_out("listening error");
}

/* Bearbeite die Verbindungswünsche von Clients
 * Der Aufruf von accept() blockiert so lange,
 * bis ein Client Verbindung aufnimmt */
void Connector::accept_socket(socket_t *socket, socket_t *new_socket) {
    struct sockaddr_in client;
    socklen_t len;

    len = sizeof(client);
    *new_socket = accept(*socket, (struct sockaddr *)&client, &len);
    if(*new_socket == -1) Logger::getInstance().error_n_out("accept error");
}

/* Daten empfangen via TCP */
void Connector::TCP_recv(socket_t *sock, char *data, size_t size) {
    int len;
    len = recv(*sock, data, size, 0);
    std::cout << data << std::endl;
    if(len > 0 || len != -1)
        data[len] = '\0';
    else
        Logger::getInstance().error_n_out("receive error");
}

void Connector::close_socket(socket_t *sock) { close(*sock); }
