using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample
{
    public class FrameMonitor
    {
        private double fSampleInterval = 0.5;
        private int fTotalFrames = 0;
        private double fTotalTime = 1;
        private int fFrames = 0;
        System.Diagnostics.Stopwatch fTiming = new System.Diagnostics.Stopwatch();
        
        public FrameMonitor()
        {
            fTiming.Reset();
            fTiming.Start();
        }

        public void Tick()
        {
            this.fFrames++;
            if (this.fTiming.Elapsed.TotalSeconds >= this.fSampleInterval)
            {
                this.fTotalTime = this.fTiming.Elapsed.TotalSeconds;
                this.fTotalFrames = this.fFrames;
                this.fFrames = 0;
                this.fTiming.Restart();
            }
        }

        public double FPS { get { return  this.fTotalFrames / this.fTotalTime;}}
    }
}
