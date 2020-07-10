#include "logger.hpp"

void Logger::error_n_out(std::string msg) {
    std::cerr << msg << ": " << strerror(errno) << std::endl;
    exit(EXIT_FAILURE);
}

void Logger::log(std::string msg) { std::cout << msg << std::endl; }