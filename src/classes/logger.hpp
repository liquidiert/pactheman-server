#ifndef LOGGER_HEADER_GUARD
#define LOGGER_HEADER_GUARD
#include <cerrno>
#include <cstring>
#include <iostream>
#include <string>

class Logger {
    Logger() = default;
    ~Logger() = default;
    Logger(const Logger&) = delete;
    Logger& operator=(const Logger&) = delete;

   public:
    // sufficent since C++11
    static Logger &getInstance() {
        static Logger instance;
        return instance;
    }

    void error_n_out(std::string);
    void log(std::string);
};
#endif  // LOGGER_HEADER_GUARD