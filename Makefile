CC= mcs
CFLAGS= -pkg:gtk-sharp-3.0
OUTDIR= ./build
OBJS= $(shell find ./src -type f -name "*.cs")
REFS= -r:Mono.Data.Sqlite.dll -r:System.Data.dll -r:build/SQLite.dll
RM= rm -f

.PHONY= all clean

all: Tabellarius.exe

Tabellarius.exe: $(OBJS)
	$(CC) $(CFLAGS) $(REFS) $^ -out:$(OUTDIR)/$@

clean:
	rm ./build/Tabellarius.exe