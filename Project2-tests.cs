using System;
using System.IO;
using Xunit;
using AirNomadHttpGrabers;
using ReportGeneratorFunctions;
using AirNomadPrompter;
using ConsoleReport;
using AirNomadReportCompile;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ReportGenerator
{

    public class GenerationTests
    {
        private const string RelPath = "../../../test_output/";
        private const string ExpRelPath = "../../../expected/";
    
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
            var HttpResults = await http.grabCourseData();
            Assert.Equal(HttpResults, System.IO.File.ReadAllText(ExpRelPath + "expected.json"));
        }

        [Fact]
        public void JsonCompilesAsExpected()
        {
            
            var HttpResults = System.IO.File.ReadAllText(ExpRelPath+"expected.json");
            //take the results from the first test and make it a json file
            var generator = new GenerateJSON(RelPath + "generated");
            var success = generator.GenerateReport(HttpResults);

            Assert.True(success);
            Assert.True(FilesAreEqual(RelPath + "generated.json", ExpRelPath + "expected.json"));

        }
        [Fact]
        public void CsvGeneratesAsExpected()
        {

            var HttpResults = System.IO.File.ReadAllText(ExpRelPath+"expected.json");
            // sets up a course grabber object for us
            var generator = new GenerateCSV(RelPath + "generated");
            var success = generator.GenerateReport(HttpResults);

            Assert.True(success);
            Assert.True(FilesAreEqual(RelPath + "generated.csv", ExpRelPath + "expected.csv"));

        }
        [Fact]
        public void HtmlGeneratesAsExpected()
        {
            var HttpResults = System.IO.File.ReadAllText(ExpRelPath+"expected.json");
            var generator = new GenerateHTML(RelPath + "generated", ExpRelPath + "boilerplate.html");
            var success = generator.GenerateReport(HttpResults);

            Assert.True(success);
            Assert.True(FilesAreEqual(RelPath + "generated.html", ExpRelPath + "expected.html"));
        }
    }

    public class EndToEndTest
    {
        private const string RelPath = "../../../test_output/";
        private const string ExpRelPath = "../../../expected/";
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
                    return new GenerateHTML(destination, ExpRelPath + "boilerplate.html");
                case "csv":
                    return new GenerateCSV(destination);
                default:
                    break;
            }

            return new GenerateJSON(destination);
        }
        [Fact]
        public async Task FullProcessRuns()
        {
            // the test will go on forever if this token is not set! To fix that issue, we will fail the test if it is not set.
            var token = Environment.GetEnvironmentVariable("API_TOKEN");
            Assert.False(token == null);

            // the test will go on forever if this token is not set! To fix that issue, we will fail the test if it is not set.
            List<Prompt> prompts = new List<Prompt>(){
                new Prompt("59796","JSON", RelPath+"full_generated.json"),
                new Prompt("59796","CSV", RelPath+"full_generated.csv"),
                new Prompt("59796","HTML", RelPath+"full_generated.html")
            };

            /*We will now initialize some objects that will be used as we go execute the call for each prompt */
            var compiler = new ReportCompile();
            CourseGrabber http = new CourseGrabber();

            var SuccessReports = new List<ReportItem>();

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
                    // Display all errors in an awesome fashion.
                    ConsoleRep.Log(new string[] { "WE GOT AN ERROR BOSS!", e.Message, "Course: " + prompt.CourseId, prompt.OutFormat + " " + prompt.Destination }, ConsoleColor.Red);
                    success = false;
                }
                Assert.True(success);
            }
                Assert.True(FilesAreEqual(prompts[0].Destination, ExpRelPath+"expected.json"));
                Assert.True(FilesAreEqual(prompts[1].Destination, ExpRelPath+"expected.csv"));
                Assert.True(FilesAreEqual(prompts[2].Destination, ExpRelPath+"expected.html"));
        }

        [Fact]
        public async Task HandlesInvalidPrompt()
        {
            // the test will go on forever if this token is not set! To fix that issue, we will fail the test if it is not set.
            var token = Environment.GetEnvironmentVariable("API_TOKEN");
            Assert.False(token == null);
            var compiler = new ReportCompile();
            var prompt = new Prompt("", "", RelPath+"");
            CourseGrabber http = new CourseGrabber(prompt.CourseId);
            compiler.CalibrateCompiler(grabReportObject(prompt.OutFormat, prompt.Destination), http);
            var res = (await compiler.CompileReport());
            Assert.True(res);
        }
    }


}


