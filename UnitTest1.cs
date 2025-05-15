using System;
using System.Diagnostics;
using printer3d;

namespace TestProject1
{
    public class Tests
    {
        MotorManager motorManager;
        List<Point> points;
        
        [SetUp]
        public async Task SetupAsync()
        {
            motorManager = new MotorManager(1, 2, 3);
            points = await PointFactory.readPointData();
        }

        [Test]
        public void TestPointAvailable()
        {
            Assert.That(points.Count, Is.EqualTo(4));
            Assert.That(points[0].xAxis, Is.EqualTo(6));
            Assert.That(points[2].yAxis, Is.EqualTo(12));
            Assert.That(points[3].zAxis, Is.EqualTo(24));
        }

        [Test]
        public async Task TestRiseDownAsync()
        {
            PointWrapper pointWrapper = new PointWrapper(new Point(12, 12, 12));
            Stopwatch stopwatch = Stopwatch.StartNew();
            double xEnd = await motorManager.motors[0].RiseDownAsync(pointWrapper.CurrentPoint.xAxis, pointWrapper, CancellationToken.None);
            stopwatch.Stop();
            Assert.That(xEnd, Is.EqualTo(0));
            Assert.That(stopwatch.Elapsed.TotalSeconds, Is.InRange(12, 13));
        }

        [Test]
        public async Task TestMoveAsync()
        {
            PointWrapper pointWrapper = new PointWrapper(new Point(6, 6, 6));
            Stopwatch stopwatch = Stopwatch.StartNew();
            double yEnd = await motorManager.motors[1].MoveAsync(pointWrapper.CurrentPoint.yAxis, 18, pointWrapper, CancellationToken.None);
            stopwatch.Stop();
            Assert.That(yEnd, Is.EqualTo(18));
            Assert.That(stopwatch.Elapsed.TotalSeconds, Is.InRange(6, 7));
        }

        [Test]
        public async Task TestRiseUpAsync()
        {
            PointWrapper pointWrapper = new PointWrapper(new Point(0, 0, 0));
            Stopwatch stopwatch = Stopwatch.StartNew();
            double zEnd = await ((MotorZ)motorManager.motors[2]).RiseUpMotor(pointWrapper.CurrentPoint.zAxis, pointWrapper, CancellationToken.None);
            stopwatch.Stop();
            Assert.That(zEnd, Is.EqualTo(12));
            Assert.That(stopwatch.Elapsed.TotalSeconds, Is.InRange(4, 5));
        }

        [Test]
        public async Task TestMultipleTaskAsync()
        {
            PointWrapper pointWrapper = new PointWrapper(new Point(0, 0, 0));
            Stopwatch[] stopwatches = new Stopwatch[3] {new(), new(), new()};
            Array.ForEach(stopwatches, s => s.Start());
            Task taskX = motorManager.motors[0].MoveAsync(pointWrapper.CurrentPoint.xAxis, 18, pointWrapper, CancellationToken.None);
            Task taskY = motorManager.motors[1].MoveAsync(pointWrapper.CurrentPoint.yAxis, 18, pointWrapper, CancellationToken.None);
            Task taskZ = motorManager.motors[2].MoveAsync(pointWrapper.CurrentPoint.zAxis, 18, pointWrapper, CancellationToken.None);
            while (!taskX.IsCompleted)
            {
                if (taskY.IsCompleted)
                {
                    stopwatches[1].Stop();
                }
                if (taskZ.IsCompleted)
                {
                    stopwatches[2].Stop();
                }
            }
            stopwatches[0].Stop();
            Assert.That(stopwatches[0].Elapsed.TotalSeconds, Is.InRange(18, 19));
            Assert.That(stopwatches[1].Elapsed.TotalSeconds, Is.InRange(9,10));
            Assert.That(stopwatches[2].Elapsed.TotalSeconds, Is.InRange(6, 7));
        }
    }
}