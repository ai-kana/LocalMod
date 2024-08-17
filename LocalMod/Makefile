build:
	dotnet build -c Release
	cp ./bin/Release/netstandard2.1/LocalMod.dll ~/Unturned/Modules/LocalMod-Module/LocalMod.dll
	cp ./bin/Release/netstandard2.1/LocalMod.dll ~/U3DS/Servers/LocalModTest/OpenMod/plugins/LocalMod.dll

clean:
	rm -rf bin
	rm -rf obj
