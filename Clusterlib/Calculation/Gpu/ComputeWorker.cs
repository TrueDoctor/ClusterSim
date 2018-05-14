using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterSim.ClusterLib.Calculation.Gpu
{
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.Remoting.Contexts;

    using ClusterSim.ClusterLib.Calculation.OpenCl;

    using OpenCL.Net.Extensions;
    using OpenCL.Net;


    public static class ComputeWorker
    {
        private static ErrorCode err;

        private static Event event0;

        private static Program program;

        static ComputeWorker()
        {
            SetupAcc();
        }

        private static Platform[] Platforms { get; } = Cl.GetPlatformIDs(out err);

        private static Device[] Devices { get; } = Cl.GetDeviceIDs(Platforms[0], DeviceType.Gpu, out err);

        private static Device Device { get; } = Devices[0]; //cl_device_id device;

        private static OpenCL.Net.Context Context { get; } =
            Cl.CreateContext(null, 1, Devices, null, IntPtr.Zero, out err);

        private static CommandQueue CmdQueue { get; set; } = Cl.CreateCommandQueue(
            Context,
            Device,
            CommandQueueProperties.None,
            out err);

        /*private static void SetupDouble()
        {
            // Create and build a program from our OpenCL-C source code
            string programSource = @"
            __kernel void doubleMe(__global double* input, __global double* output) 
            { 
                size_t i = get_global_id(0);
                output[i] = input[i] * input[i];
            };";
            program = Cl.CreateProgramWithSource(Context, 1, new[] { programSource }, null, out err);
            Cl.BuildProgram(program, 0, null, string.Empty, null, IntPtr.Zero);  //"-cl-mad-enable"


            // Check for any compilation errors
            if (Cl.GetProgramBuildInfo(program, Device, ProgramBuildInfo.Status, out err).CastTo<BuildStatus>() != BuildStatus.Success)
            {
                if (err != ErrorCode.Success)
                    Console.WriteLine("ERROR: " + "Cl.GetProgramBuildInfo" + " (" + err.ToString() + ")");
                Console.WriteLine("Cl.GetProgramBuildInfo != Success");
                Console.WriteLine(Cl.GetProgramBuildInfo(program, Device, ProgramBuildInfo.Log, out err));
            }
        }
*/

        private static void SetupAcc()
        {

            // Create and build a program from our OpenCL-C source code
            string programSource = File.ReadAllText(
                @"C:\Users\Dennis\Source\Repos\ClusterSim\Clusterlib\Calculation\Gpu\Test.cl");
            program = Cl.CreateProgramWithSource(Context, 1, new[] { programSource }, null, out err);
            Cl.BuildProgram(program, 0, null, @"-g -s C:\Users\Dennis\Source\Repos\ClusterSim\Clusterlib\Calculation\Gpu\Test.cl", null, IntPtr.Zero); //" - cl-mad-enable"


            // Check for any compilation errors
            if (Cl.GetProgramBuildInfo(program, Device, ProgramBuildInfo.Status, out err).CastTo<BuildStatus>()
                != BuildStatus.Success)
            {
                if (err != ErrorCode.Success)
                    Console.WriteLine("ERROR: " + "Cl.GetProgramBuildInfo" + " (" + err.ToString() + ")");
                Console.WriteLine("Cl.GetProgramBuildInfo != Success");
                Console.WriteLine(Cl.GetProgramBuildInfo(program, Device, ProgramBuildInfo.Log, out err));
            }
        }

        /*public static void CalcDouble()
        {
            const int count = 2048;

            // Lets create an random array of floats
            var random = new Random();
            double[] data = (from i in Enumerable.Range(0, count) select (double)random.NextDouble()).ToArray();

            // Create a kernel from our program
            Kernel kernel = Cl.CreateKernel(program, "doubleMe", out err);


            // Allocate input and output buffers, and fill the input with data
            Mem memInput = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadOnly, sizeof(double) * count, out err);


            // Create an output memory buffer for our results
            Mem memoutput = (Mem)Cl.CreateBuffer(Context, MemFlags.WriteOnly, sizeof(double) * count, out err);


            // Copy our host buffer of random values to the input device buffer
            Cl.EnqueueWriteBuffer(CmdQueue, (IMem)memInput, Bool.True, IntPtr.Zero, new IntPtr(sizeof(double) * count), data, 0, null, out event0);


            // Get the maximum number of work items supported for this kernel on this device
            IntPtr notused;
            InfoBuffer local = new InfoBuffer(new IntPtr(4));
            Cl.GetKernelWorkGroupInfo(kernel, Device, KernelWorkGroupInfo.WorkGroupSize, new IntPtr(sizeof(int)), local, out notused);


            // Set the arguments to our kernel, and enqueue it for execution
            Cl.SetKernelArg(kernel, 0, new IntPtr(4), memInput);
            Cl.SetKernelArg(kernel, 1, new IntPtr(4), memoutput);
            Cl.SetKernelArg(kernel, 2, new IntPtr(4), new IntPtr(42));
            IntPtr[] workGroupSizePtr = new IntPtr[] { new IntPtr(count) };
            Cl.EnqueueNDRangeKernel(CmdQueue, kernel, 1, null, workGroupSizePtr, null, 0, null, out event0);


            // Force the command queue to get processed, wait until all commands are complete
            Cl.Finish(CmdQueue);


            // Read back the results
            double[] results = new double[count];
            Cl.EnqueueReadBuffer(CmdQueue, (IMem)memoutput, Bool.True, IntPtr.Zero, new IntPtr(count * sizeof(double)), results, 0, null, out event0);


            // Validate our results
            int correct = 0;
            for (int i = 0; i < count; i++)
                correct += (Math.Abs(results[i] - data[i] * data[i]) < 1e-16) ? 1 : 0;
        }*/

        /*public static void CalcAcc(List<Star> stars, List<IMassive> bodys)
        {
            var instruction = new List<int>[stars.Count];
            foreach (var star in stars)
            {
                var ids = new List<int>();
                for (int i = 0; i < stars.Count; i++)
                {
                    if (star.id != i)
                    {
                        ids.Add(i);
                    }
                }

                instruction[star.id] = ids.ToArray().ToList();
            }

            CalcAcc(stars, bodys, instruction);
        }*/

        public static void CalcAcc(List<Star> stars, List<IMassive> bodys, Dictionary<List<int>, int[]> cluster)
        {
            int count = stars.Count;
            int
                instructionCount; //= instructions.Sum(ids => ids.Value.Length);   //pass array with workgrop size + offset -> instruction array instruction buffer multiple of workgroupsize

            // Create a kernel from our program
            Kernel kernel = Cl.CreateKernel(program, "calcAcc", out err);

            var Stars = new Star[stars.Count];
            var instructions = new List<int>();
            var WorkSizes = new List<int>();
            var WorkGroupOffset = new List<int>();
            var WorkGroupSizes = new List<int>();
            int WorkGroupCount = cluster.Count;

            int k = 0;
            foreach (var intse in cluster)
            {
                foreach (int i in intse.Key)
                {
                    Stars[k++] = stars[i];
                }

                WorkGroupSizes.Add(intse.Key.Count);
                WorkGroupOffset.Add(instructions.Count);
                WorkSizes.Add(intse.Value.Length + intse.Value.Length % intse.Key.Count);

                instructions.AddRange(intse.Value);
                instructions.AddRange(Enumerable.Repeat(-1, intse.Value.Length % intse.Key.Count));
            }

            instructionCount = instructions.Count;


            // Allocate input and output buffers, and fill the input with data
            //Mem memInputStar = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadOnly, sizeof(double), out err);

            Mem memInput = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadOnly, sizeof(double) * 4 * count, out err);
            Mem memBodys = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadOnly, sizeof(double) * 4 * bodys.Count, out err);
            Mem memInstructions = (Mem)Cl.CreateBuffer(
                Context,
                MemFlags.ReadOnly,
                sizeof(int) * instructionCount,
                out err);
            Mem memfrom = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadOnly, sizeof(int) * WorkGroupCount, out err);
            Mem memto = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadOnly, sizeof(int) * WorkGroupCount, out err);
            Mem memGroupSize = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadOnly, sizeof(int) * WorkGroupCount, out err);
            Mem memBuffer = (Mem)Cl.CreateBuffer(Context, MemFlags.ReadWrite, sizeof(int) * 256, out err);
            // Create an output memory buffer for our results
            //Mem memoutput = (Mem)Cl.CreateBuffer(Context, MemFlags.WriteOnly, sizeof(double)  * count, out err);
            Mem memoutput = (Mem)Cl.CreateBuffer(Context, MemFlags.WriteOnly, sizeof(double) * 4 * count, out err);


            // Copy our host buffer of random values to the input device buffer
            //Cl.EnqueueWriteBuffer(CmdQueue, (IMem)memInputStar, Bool.True, IntPtr.Zero, new IntPtr(sizeof(double)), 42.0/* bodys.Select(x=>x.mass).ToArray()*/, 0, null, out event0);
            Cl.EnqueueWriteBuffer(
                CmdQueue,
                memInput,
                Bool.True,
                IntPtr.Zero,
                new IntPtr(sizeof(double) * 4 * count),
                stars.Select(x => x.GetDouble4()).ToArray(),
                0,
                null,
                out event0);

            Cl.EnqueueWriteBuffer(
                CmdQueue,
                memBodys,
                Bool.True,
                IntPtr.Zero,
                new IntPtr(sizeof(double) * 4 * count),
                bodys.Select(x => x.GetDouble4()).ToArray(),
                0,
                null,
                out event0);

            var n = new List<int>();
            int[] from = new int[count];
            int[] countInts = new int[count];
            /*for (var index = 0; index < instructions.Length; index++)
            {
                var s = instructions[index];
                from[index] = n.Count;
                countInts[index] = s.Count;
                n.AddRange(s);
            }*/


            int[] insArr = n.ToArray();

            Cl.EnqueueWriteBuffer(
                CmdQueue,
                memInstructions,
                Bool.True,
                IntPtr.Zero,
                new IntPtr(sizeof(int) * instructionCount),
                insArr,
                0,
                null,
                out event0);

            Cl.EnqueueWriteBuffer(
                CmdQueue,
                memfrom,
                Bool.True,
                IntPtr.Zero,
                new IntPtr(sizeof(int) * WorkGroupCount),
                WorkGroupOffset.ToArray(),
                0,
                null,
                out event0);

            Cl.EnqueueWriteBuffer(
                CmdQueue,
                memto,
                Bool.True,
                IntPtr.Zero,
                new IntPtr(sizeof(int) * WorkGroupCount),
                WorkSizes.ToArray(),
                0,
                null,
                out event0);


            Cl.EnqueueWriteBuffer(
                CmdQueue,
                memGroupSize,
                Bool.True,
                IntPtr.Zero,
                new IntPtr(sizeof(int) * WorkGroupCount),
                WorkGroupSizes.ToArray(),
                0,
                null,
                out event0);


            // Get the maximum number of work items supported for this kernel on this device
            IntPtr notused;
            InfoBuffer local = new InfoBuffer(new IntPtr(4));
            Cl.GetKernelWorkGroupInfo(
                kernel,
                Device,
                KernelWorkGroupInfo.WorkGroupSize,
                new IntPtr(sizeof(int)),
                local,
                out notused);

            double a = 42;

            // Set the arguments to our kernel, and enqueue it for execution
            var error = Cl.SetKernelArg(kernel, 0, new IntPtr(4), memInput);
            error = Cl.SetKernelArg(kernel, 1, new IntPtr(4), memBodys);
            error = Cl.SetKernelArg(kernel, 2, new IntPtr(4), memInstructions);
            error = Cl.SetKernelArg(kernel, 3, new IntPtr(4), memfrom);
            error = Cl.SetKernelArg(kernel, 4, new IntPtr(4), memto);
            error = Cl.SetKernelArg(kernel, 5, new IntPtr(4), memGroupSize);
            error = Cl.SetKernelArg(kernel, 6, new IntPtr(4), memoutput);
            error = Cl.SetKernelArg(kernel, 7, new IntPtr(sizeof(double)), a);
            error = Cl.SetKernelArg(kernel, 8, new IntPtr(sizeof(double) * 4 * 256), null);
            //Cl.SetKernelArg(kernel, 3, new IntPtr(4), count);
            //IntPtr[] workGroupSizePtr = new IntPtr[] { new IntPtr(count) };
            var workGroupSizePtr = new[] { new IntPtr(count) };

            Cl.EnqueueNDRangeKernel(CmdQueue, kernel, 1, null, workGroupSizePtr, null, 0, null, out event0);


            // Force the command queue to get processed, wait until all commands are complete
            Cl.Finish(CmdQueue);


            // Read back the results
            double4[] results = new double4[count];
            Cl.EnqueueReadBuffer(
                CmdQueue,
                (IMem)memoutput,
                Bool.True,
                IntPtr.Zero,
                new IntPtr(count * 4 * sizeof(double)),
                results,
                0,
                null,
                out event0);


            // Validate our results
            int correct = 0;
            for (int i = 0; i < count; i++)
            {
            }
        }
    }
}
