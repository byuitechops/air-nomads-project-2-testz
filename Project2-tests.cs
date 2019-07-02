using System;
using System.IO;
using Xunit;
using AirNomadHttpGrabers;
using ReportGeneratorFunctions;
using AirNomadPrompter;
using ConsoleReport;
using AirNomadReportCompile;

namespace ReportGenerator
{
    public class JSONTests
    {
        [Fact]
        public async void ActualAndExpectedShouldBeEqual()
        {
            var compiler = new ReportCompile();
            var http = new CourseGrabber();
            var promptObj = new Prompt("59796", "JSON", "../../../../air-nomads-project-2-testz/generated.json");
            http.CourseID = promptObj.CourseId;
            compiler.CalibrateCompiler(promptObj, new GenerateJSON(promptObj.Destination), http);
            var success = await compiler.CompileReport();
            // System.Console.WriteLine(success);

            if (success)
            {
                var actual = File.ReadAllText("../../../../air-nomads-project-2-testz/generated.json");
                var expected = File.ReadAllText("../../../../air-nomads-project-2-testz/expected.json");

                Assert.Equal(expected, actual);
            }
        }
    }

    public class CSVTests
    {
        [Fact]
        public void Test1()
        {

        }
    }

    public class HTMLTests
    {
        [Fact]
        public void Test1()
        {

        }
    }
}
