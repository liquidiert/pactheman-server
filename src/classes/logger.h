#include <cerrno>
#include <cstring>
#include <iostream>
#include <string>

class Logger {
   public:
    static Logger& getInstance();
    void error_n_out(std::string);
    void log(std::string);
};