.PHONY: build

build:
	dotnet publish -c Release -o detector-server-1.0/opt/gvaduha/detector-server-1.0/ ../SharpStresser/SharpStresser.csproj
	dpkg-deb --build detector-server-1.0
