CXX=g++
CXX_FLAGS=-Wall
SRC_DIR=src
OBJ_DIR=src/obj
OUT_DIR=out
# reserved for needed headers
DEPENDENCIES=
_OBJ=server.o logger.o connector.o main.o
OBJ=$(patsubst %,$(OBJ_DIR)/%,$(_OBJ))

$(OBJ_DIR)/%.o: $(SRC_DIR)/%.cpp
	$(CXX) $(CXX_FLAGS) -c $^ -o $@

server: $(OBJ)
	$(CXX) $(CXX_FLAGS) -o $(OUT_DIR)/$@ $^

.PHONY: run

run:
	./$(OUT_DIR)/server

.PHONY: clean

clean:
	rm $(OUT_DIR)/*
	rm $(OBJ_DIR)/*.o