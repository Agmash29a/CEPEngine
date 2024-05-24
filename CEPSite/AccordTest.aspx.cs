using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Accord.Controls;
using Accord.MachineLearning.VectorMachines.Learning;
using Accord.Math.Optimization.Losses;
using Accord.Statistics;
using Accord.Statistics.Kernels;
using Accord.MachineLearning;

namespace CEPSite
{
    public partial class AccordTest : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            double[][] inputs =
   {
                /* 1.*/ new double[] { 0, 0 },
                /* 2.*/ new double[] { 1, 0 }, 
                /* 3.*/ new double[] { 0, 1 }, 
                /* 4.*/ new double[] { 1, 1 },
            };

            int[] outputs =
            { 
                /* 1. 0 xor 0 = 0: */ 0,
                /* 2. 1 xor 0 = 1: */ 1,
                /* 3. 0 xor 1 = 1: */ 1,
                /* 4. 1 xor 1 = 0: */ 0,
            };

            // Create the learning algorithm with the chosen kernel
            var smo = new SequentialMinimalOptimization<Gaussian>()
            {
                Complexity = 100 // Create a hard-margin SVM 
            };

            // Use the algorithm to learn the svm
            var svm = smo.Learn(inputs, outputs);

            // Compute the machine's answers for the given inputs
            bool[] prediction = svm.Decide(inputs);

            // Compute the classification error between the expected 
            // values and the values actually predicted by the machine:
            double error = new AccuracyLoss(outputs).Loss(prediction);

            //Console.WriteLine("Error: " + error);

            //// Show results on screen 
            ////ScatterplotBox.Show("Training data", inputs, outputs);
            ////ScatterplotBox.Show("SVM results", inputs, prediction.ToZeroOne());

            //Console.ReadKey();

            Accord.Math.Random.Generator.Seed = 0;

            // Declare some observations
            double[][] observations =
            {
                new double[] { -5, -2, -1, -5, -5, -6  },
                new double[] { -5, -5, -6, -5, -5, -6 },
                new double[] {  2,  1,  1, -5, -5, -6 },
                new double[] {  1,  1,  2, -5, -5, -6 },
                new double[] {  1,  2,  2, -5, -5, -6 },
                new double[] {  3,  1,  2, -5, -5, -6 },
                new double[] { 11,  5,  4, -5, -5, -6 },
                new double[] { 15,  5,  6, -5, -5, -6 },
                new double[] { 10,  5,  6, -5, -5, -6 },
            };

            // Create a new K-Means algorithm
            KMeans kmeans = new KMeans(k: 5);

            // Compute and retrieve the data centroids
            var clusters = kmeans.Learn(observations);

            // Use the centroids to parition all the data
            int[] labels = clusters.Decide(observations);

            testing(a: 7);
        }

        public void testing(int a)
        {
            int x = a;
        }
    }
}