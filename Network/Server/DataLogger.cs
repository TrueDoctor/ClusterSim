using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.Net.Server
{
    using System.Threading;

    using ClusterSim.ClusterLib.Analysis;

    public class DataLogger
    {
        public Queue<double> Predicted { get; set; } = new Queue<double>();

        public Queue<double> PredictedSub { get; set; } = new Queue<double>();

        public Queue<double> Result { get; set; } = new Queue<double>();

        public void Log(double predicted, double predictedSub, double result)
        {
            this.Predicted.Enqueue((int)predicted);
            this.PredictedSub.Enqueue((int)predictedSub);
            this.Result.Enqueue((int)result);

            if (this.Predicted.Count <= 100)
            {
                return;
            }

            this.Predicted.Dequeue();
            this.PredictedSub.Dequeue();
            this.Result.Dequeue();


            GnuPlot.HoldOn();
            GnuPlot.Set("ylabel 'Rechenzeit in s', font ',10'", "tics font ', 10'", "logscale y 10");
            /*GnuPlot.Plot(this.Predicted.ToArray(), "title 'Predicted' w linespoints");
            Thread.Sleep(2000);
            GnuPlot.Plot(this.PredictedSub.ToArray(), "title 'PredictedSub' w linespoints");
            Thread.Sleep(2000);*/
            GnuPlot.Plot(this.Result.ToArray(), "title 'Rechenzeit' w linespoints");
            Thread.Sleep(4000);
            GnuPlot.HoldOff();
        }
    }
}
