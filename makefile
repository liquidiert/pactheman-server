CXX=g++
CXX_FLAGS=-Wall
DEBUG_FLAGS=-g
SRC_DIR=src
OBJ_DIR=src/obj
OUT_DIR=out
SRC_FILES=$(SRC_DIR)/main.cpp $(SRC_DIR)/classes/server.cpp $(SRC_DIR)/classes/logger.cpp $(SRC_DIR)/classes/connector.cpp
OBJ_FILES=$(OBJ_DIR)/main.o $(OBJ_DIR)/server.o $(OBJ_DIR)/logger.o $(OBJ_DIR)/connector.o

server: $(OBJ_FILES)
	$(CXX) $(CXX_FLAGS) $^ -o $(OUT_DIR)/$@

.PHONY: debug

debug: $(OBJ_FILES)
	$(CXX) $(CXX_FLAGS) $(DEBUG_FLAGS) $^ -o $(OUT_DIR)/$@ 

.PHONY: run

$(OBJ_DIR)/main.o: $(SRC_DIR)/main.cpp
	$(CXX) $(CXX_FLAGS) -c -o $@ $^

$(OBJ_DIR)/server.o: $(SRC_DIR)/classes/server.cpp
	$(CXX) $(CXX_FLAGS) -c -o $@ $^

$(OBJ_DIR)/logger.o: $(SRC_DIR)/classes/logger.cpp
	$(CXX) $(CXX_FLAGS) -c -o $@ $^

$(OBJ_DIR)/connector.o: $(SRC_DIR)/classes/connector.cpp
	$(CXX) $(CXX_FLAGS) -c -o $@ $^

run:
	./$(OUT_DIR)/server

.PHONY: clean

clean:
	rm $(OBJ_DIR)/*.o
	rm $(OUT_DIR)/*