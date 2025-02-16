using NUnit.Framework;
using AudioFile.Controller;

namespace AudioFile.Tests
{
    [TestFixture]
    public class FindTrackOnWhoSampled
    {
        [Test]
        public void ExampleTest()
        {
            int expected = 4;
            int actual = 2 + 2;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public async Task RealHttpRequest_ReturnsHtmlResponse()
        {
            //WhoSampledSearcher searcher = new WhoSampledSearcher();
            var trackSampleController = TrackSampleController.Instance;

            var result = await searcher.SearchTrackAsync("MF DOOM");

            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);

            Console.WriteLine("Real request results: " + string.Join(", ", result));
        }

    }
}

