CC= mcs
CFLAGS= -pkg:gtk-sharp-3.0
OUTDIR= ./build
OBJS= $(shell find ./src ./Properties -type f -name "*.cs")
REFS= -r:Mono.Data.Sqlite.dll -r:System.Data.dll -r:ext/sqlite-net/SQLite.dll
RM= rm -f

.PHONY= all clean

all: Tabellarius.exe

Release: $(OBJS)
	$(CC) $(CFLAGS) $(REFS) -optimize -debug- $^ -out:$(OUTDIR)/Release/Tabellarius.exe

Tabellarius.exe: $(OBJS)
	$(CC) $(CFLAGS) $(REFS) -debug+ $^ -out:$(OUTDIR)/Debug/$@

clean:
	rm ./build/Tabellarius.exe