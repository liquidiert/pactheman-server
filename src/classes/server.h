#include <iostream>
#include <string>

#include "connector.h"
#include "logger.h"

class Server : Connector {
   public:
    Server() : Connector() {}

    void main_loop();
};