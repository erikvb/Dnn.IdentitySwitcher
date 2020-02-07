echo "This assumes that you are at Visual Studio command prompt, and in the top level directory for the solution"
echo "Executes AfterBuild release build and then packages the module in the Installation directory"

msbuild -t:Rebuild;AfterBuild -p:Configuration=Release IdentitySwitcher.sln