#include "logger.h"

class Logger {
    Logger() = default;
    ~Logger() = default;
    Logger(const Logger&) = delete;
    Logger& operator=(const Logger&) = delete;

   public:
    // sufficent since C++11
    static Logger& getInstance() {
        static Logger instance;
        return instance;
    }

    void error_n_out(std::string msg) {
        std::cerr << msg << ": " << strerror(errno) << std::endl;
        exit(EXIT_FAILURE);
    }

    void log(std::string msg) { std::cout << msg << std::endl; }
};