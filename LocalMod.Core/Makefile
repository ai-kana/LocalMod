BDIR := bin/Release/netstandard2.1
CORE := LocalMod.Core.dll 
API := LocalMod.API.dll
PDB := LocalMod.Core.pdb 

MODULE := ../LocalMod.Module
GAME := ~/Unturned/Modules/LocalMod.Module

LIB := ../LocalMod.Module/Libs

DEPS := \
	UniTask.dll \
	Autofac.dll \
	Microsoft.Extensions.DependencyInjection.dll \
	Microsoft.Extensions.DependencyInjection.Abstractions.dll \
	Microsoft.Extensions.Configuration.dll \
	Microsoft.Extensions.Configuration.Json.dll \
	Microsoft.Extensions.Configuration.Binder.dll \
	Microsoft.Extensions.Configuration.Abstractions.dll \
	Microsoft.Extensions.Configuration.FileExtensions.dll \
	Microsoft.Extensions.FileProviders.Abstractions.dll \
	Microsoft.Extensions.FileProviders.Physical.dll \
	Microsoft.Extensions.Logging.dll \
	Microsoft.Extensions.Options.dll \
	Microsoft.Extensions.Logging.Abstractions.dll \
	Microsoft.Extensions.Primitives.dll \
	Microsoft.Extensions.FileSystemGlobbing.dll \
	Microsoft.Bcl.AsyncInterfaces.dll \
	System.Memory.dll \
	System.Buffers.dll \
	System.Text.Json.dll \
	System.Numerics.Vectors.dll \
	System.Threading.Tasks.Extensions.dll \
	System.Text.Encodings.Web.dll \
	System.Runtime.CompilerServices.Unsafe.dll \
	System.Diagnostics.DiagnosticSource.dll \
	System.ComponentModel.Composition.dll \
	MonoMod.ILHelpers.dll \

build:
	dotnet build -c Release
	cp $(BDIR)/$(CORE) $(MODULE)/$(CORE)
	cp $(BDIR)/$(PDB) $(MODULE)/$(PDB)
	cp $(BDIR)/$(API) $(MODULE)/$(API)
	make cpmod
	make -C ../LocalMod.Module

cplib:
	@for file in $(DEPS); do \
		cp $(BDIR)/$$file $(LIB)/$$file; \
		echo "Copied $$file";\
	done

cpmod: cplib
	rm -rf $(GAME)
	cp -r $(MODULE) $(GAME)

.PHONY: build cplib cpmod
