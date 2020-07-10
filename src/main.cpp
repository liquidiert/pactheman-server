#include "classes/server.hpp"

int main(int argc, char **argv) {
    Server *server = new Server();
    server->main_loop();
    return EXIT_SUCCESS;
}