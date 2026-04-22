
OUTPUT_DIRECTORY := ./bin/
TARGET_FILE := main.exe
SUB_TARGET_OBJECTS := $(patsubst %.cc,%.o,$(wildcard ./source/*.cc))

$(TARGET_FILE) : $(OUTPUT_DIRECTORY)main.o $(SUB_TARGET_OBJECTS)
	g++ $^ -o $@ -lgdi32

$(OUTPUT_DIRECTORY)main.o : main.cc
	if not exist bin mkdir bin
	g++ -S $^ -o $(OUTPUT_DIRECTORY)main.s
	as $(OUTPUT_DIRECTORY)main.s -o $@ -g

.PHONY:
clean :
	del .\bin /Q
	del .\source\*.o /Q