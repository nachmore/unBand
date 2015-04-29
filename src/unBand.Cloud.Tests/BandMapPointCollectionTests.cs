using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ploeh.AutoFixture;
using unBand.Cloud.Events;

namespace unBand.Cloud.Tests
{
    [TestClass]
    public class BandMapPointCollectionTests
    {
        private IFixture _fixture;
        private BandMapPointCollection _subjectUnderTest;

        [TestInitialize]
        public void SetUp()
        {
            _fixture = new Fixture();
        }

        [TestMethod]
        public void FiltersInvalidBeginMiddleEndMapPointFromCollection()
        {
            DoTest(3, 0);
            DoTest(3, 1);
            DoTest(3, 2);
        }

        private void DoTest(int collectionSize, int indexOfInvalidMapPoint)
        {
            _subjectUnderTest = new BandMapPointCollection();
            var mapPoints = CreateTestData(collectionSize);
            MakeInvalid(mapPoints[indexOfInvalidMapPoint]);
            PopulateCollection(mapPoints);

            Assert.AreEqual(_subjectUnderTest.Count(), collectionSize - 1);
            Assert.IsFalse(_subjectUnderTest.Contains(mapPoints[indexOfInvalidMapPoint]));
        }

        private List<BandMapPoint> CreateTestData(int repeatCount)
        {
            _fixture.RepeatCount = repeatCount;
            return _fixture.CreateMany<BandMapPoint>().ToList();
        }

        private void PopulateCollection(List<BandMapPoint> data)
        {
            data.ForEach(x => _subjectUnderTest.Add(x));
        }

        private void MakeInvalid(BandMapPoint subject)
        {
            subject.Latitude = 0.0;
            subject.Longitude = 0.0;
            subject.Altitude = 0.0;
        }
    }
}