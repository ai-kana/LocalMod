build:
	dotnet build -c Release
	cp ./bin/Release/netstandard2.1/TestPlugin.dll ~/Unturned/LocalMod/Plugins/TestPlugin.dll
	cp ./bin/Release/netstandard2.1/TestPlugin.dll ~/U3DS/Servers/LocalModTest/OpenMod/plugins/LocalMod.Loader/TestPlugin.dll

clean:
	rm -rf bin
	rm -rf obj
