cd "Deepflow.Ingestion.Service"
dotnet publish -c Release -r win10-x64
cd ..

cd "Deepflow.Platform.Agent"
dotnet publish -c Release -r win10-x64
cd ..
cd "Deepflow.Platform.Sources.FakeSource"
dotnet publish -c Release -r win10-x64
cd ..