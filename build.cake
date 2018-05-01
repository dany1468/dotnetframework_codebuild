#addin "Cake.FileHelpers"

#tool "nuget:?package=OpenCover"
#tool "nuget:?package=NUnit.ConsoleRunner"

var target = Argument("target", "def");

Task("def").Does(() => {
    Information("Success");
});

Task("restore").Does(() => {
    Information("Restoring...");

    NuGetRestore("./CodeBuildTest.sln");
});

Task("build").IsDependentOn("restore").Does(() => {
    Information("Build project...");

	MSBuild(
		@".\CodeBuildTest.sln",
        new MSBuildSettings {
            Verbosity = Verbosity.Minimal,
            Configuration = "Debug"
        }
    );
});

Task("test").IsDependentOn("build").Does(() => {
  Information("Process Test...");

  NUnit3(@"./CodeBuildTest/bin/Debug/CodeBuildTest.dll",
    new NUnit3Settings {
      NoResults = true
    });

  Information("Finish test");
});

//Task("coverage").IsDependentOn("build").Does(() =>
Task("coverage").Does(() =>
{
    Information("Process OpenCover...");

	var openCoverSettings = new OpenCoverSettings()
        {
            Register = "user",
            SkipAutoProps = true,
            ArgumentCustomization = args => args.Append("-coverbytest:Test*.dll").Append("-mergebyhash")
        };

        var outputFile = new FilePath("./GeneratedReports/CalculatedReport.xml");

        OpenCover(tool => {
	        tool.NUnit3(@"./CodeBuildTest/bin/Debug/CodeBuildTest.dll",
		    new NUnit3Settings {
		        ShadowCopy = false
	    	}
		);
	    },
	    outputFile,
	    openCoverSettings
		.WithFilter("+[CodeBuildTest*]*")
	);
});

RunTarget(target);
