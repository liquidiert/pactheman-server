#ifndef SERVER_HEADER
#define SERVER_HEADER
#define BUF 1024
#include <iostream>
#include <string>

#include "connector.hpp"
#include "logger.hpp"

class Server : Connector {
    char *buffer = (char *)malloc(BUF);
   public:
    Server() : Connector() { init(); }

    void main_loop();
};
#endif  // SERVER_HEADER