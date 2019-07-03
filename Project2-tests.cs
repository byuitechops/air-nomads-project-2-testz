using System;
using System.IO;
using Xunit;
using AirNomadHttpGrabers;
using ReportGeneratorFunctions;
using AirNomadPrompter;
using ConsoleReport;
using AirNomadReportCompile;
using System.Collections.Generic;

namespace ReportGenerator
{

    public class GenerationTests
    {
        private const string RelPath = "../../../test_output/";
        private static string HttpResults;
        private static bool FilesAreEqual(string path1, string path2)
        {
            var actual = File.ReadAllText(path1);
            var expected = File.ReadAllText(path2);
            return actual.Equals(expected);
        }


        [Fact]
        public async void ResultsFromHttpGetAreEqual()
        {
            // the test will go on forever if this token is not set! To fix that issue, we will fail the test if it is not set.
            var token = Environment.GetEnvironmentVariable("API_TOKEN");
            Assert.False(token == null);
            var http = new CourseGrabber("59796");
            HttpResults = await http.grabCourseData();

            Assert.Equal(HttpResults, System.IO.File.ReadAllText(RelPath + "expected.json"));
        }

        [Fact]
        public void JsonCompilesAsExpected()
        {
            Assert.False(HttpResults == null);

            var generator = new GenerateJSON(RelPath + "generated");
            var success = generator.GenerateReport(HttpResults);

            Assert.True(success);
            Assert.True(FilesAreEqual(RelPath + "generated.json", RelPath + "expected.json"));

        }
        [Fact]
        public void CsvGeneratesAsExpected()
        {

            Assert.False(HttpResults == null);
            // sets up a course grabber object for us
            var generator = new GenerateCSV(RelPath + "generated");
            var success = generator.GenerateReport(HttpResults);

            Assert.True(success);
            Assert.True(FilesAreEqual(RelPath + "generated.csv", RelPath + "expected.csv"));

        }
        [Fact]
        public void HtmlGeneratesAsExpected()
        {
            Assert.False(HttpResults == null);
            var generator = new GenerateHTML(RelPath + "generated", RelPath + "boilerplate.html");
            var success = generator.GenerateReport(HttpResults);

            Assert.True(success);
            Assert.True(FilesAreEqual(RelPath + "generated.html", RelPath + "expected.html"));
        }
    }

    public class EndToEndTest
    {
        private const string RelPath = "../../../test_output/";
        private static bool FilesAreEqual(string path1, string path2)
        {
            var actual = File.ReadAllText(path1);
            var expected = File.ReadAllText(path2);
            return actual.Equals(expected);
        }
        public static IReport grabReportObject(string type, string destination)
        {

            switch (type.ToLower().Trim())
            {
                case "json":
                    return new GenerateJSON(destination);
                case "html":
                    return new GenerateHTML(destination, RelPath + "boilerplate.html");
                case "csv":
                    return new GenerateCSV(destination);
                default:
                    break;
            }

            return new GenerateJSON(destination);
        }
        public async void FullProcessRuns()
        {
            List<Prompt> prompts = new List<Prompt>(){
                new Prompt("59796","JSON",RelPath+"full_generated.json"),
                new Prompt("59796","CSV",RelPath+"full_generated.csv"),
                new Prompt("59796","HTML",RelPath+"full_generated.html")
            };

            /*We will now initialize some objects that will be used as we go execute the call for each prompt */
            var compiler = new ReportCompile();
            CourseGrabber http = new CourseGrabber();

            /*Loop through each prompt, set up the http call, calibrate how the compiler should work and send the success reports to the Dictionary we have for keeping track of it */
            foreach (var prompt in prompts)
            {
                http.CourseID = prompt.CourseId;
                compiler.CalibrateCompiler(prompt, grabReportObject(prompt.OutFormat, prompt.Destination), http);

                var success = false;
                try
                {
                    success = await compiler.CompileReport();
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.Message);
                    // Display all errors in an awesome fashion.
                    success = false;
                }
                Assert.True(success);
                Assert.True(FilesAreEqual(RelPath+"expected.json", prompts[0].Destination));
                Assert.True(FilesAreEqual(RelPath+"expected.csv", prompts[0].Destination));
                Assert.True(FilesAreEqual(RelPath+"expected.html", prompts[0].Destination));
                
            }

        }

        [Fact]
        public async void InvalidPromptFails()
        {
            var compiler = new ReportCompile();
            CourseGrabber http = new CourseGrabber("59796");
            var prompt = new Prompt("", "", "");
            compiler.CalibrateCompiler(grabReportObject(prompt.OutFormat, prompt.Destination), http);
            Assert.False(await compiler.CompileReport());
        }
    }


}


