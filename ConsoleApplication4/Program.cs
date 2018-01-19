using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApplication4
{
    class Program
    {
        

        static void Main(string[] args)
        {
            //args for inputs
            String signalname = args[0];
            String responsename = args[1];
            String output = args[2];

            //lists of shorts made from separated byte pairs
            List<short> sourceBytes = new List<short>();
            List<short> addBytes = new List<short>();

            //two readers for the source and the implulse
            BinaryReader sourceread = new BinaryReader(File.Open(signalname, FileMode.Open));
            BinaryReader addread = new BinaryReader(File.Open(responsename, FileMode.Open));

            int position = 0;

            //turn the files into lists of shorts
            int sound_len = (int)sourceread.BaseStream.Length;
            while(position < sound_len)
            {
                short s = sourceread.ReadInt16();
                sourceBytes.Add(s);
                position += sizeof(short);
            }

            position = 0;
            sound_len = (int)addread.BaseStream.Length;

            while(position < sound_len)
            {
                short s = addread.ReadInt16();
                addBytes.Add(s);
                position += sizeof(short);
            }

            Console.WriteLine(sourceBytes.Count());
            Console.WriteLine(addBytes.Count());

            //get the larger and pad them to be the same size
            double n = 0;
            double power = 0;
            while (true)
            {
                power = Math.Pow(2, n);
                if (sourceBytes.Count > addBytes.Count && sourceBytes.Count < power)
                {
                    break;
                }
                else
                {
                    n++;
                }
                if (addBytes.Count > sourceBytes.Count && addBytes.Count < power)
                {
                    break;
                }
                else
                {
                    n++;
                }
            }
            int sigInt = sourceBytes.Count;
            int addInt = addBytes.Count;

            while(sigInt < power)
            {
                sourceBytes.Add(0);
                sigInt++;
            }
            while(addInt < power)
            {
                addBytes.Add(0);
                addInt++;
            }

            //Create arrays to hold the Complexes
            Complex[] newSource = new Complex[sigInt];
            Complex[] newAdd = new Complex[addInt];
            for(int i = 0; i < sigInt; i++)
            {
                newSource[i] = new Complex(sourceBytes[i], 0);
                newAdd[i] = new Complex(addBytes[i], 0);
            }

            //Fast-Fourier YOYOYOYO
            Complex[] FFTSource = FFT(newSource);
            Complex[] FFTAdd = FFT(newAdd);

            //combine to form Gurren Lagannn
            Complex[] combinedFFT = new Complex[sigInt];
            for (int i = 0; i < FFTSource.Length; i++)
            {
                combinedFFT[i] = FFTSource[i] * FFTAdd[i];
            }

            //fix it
            for (int i = 0; i < combinedFFT.Length; i++)
            {
                combinedFFT[i] = Complex.Conj(combinedFFT[i]);
            }

            Complex[] finalFFT = FFT(combinedFFT);

            //Get them into usable form
            for(int i = 0; i < finalFFT.Length; i++)
            {
                finalFFT[i] = new Complex(finalFFT[i].Re / n, finalFFT[i].Im / n);
            }

            List<double> finalSig = new List<double>();

            foreach(Complex c in finalFFT)
            {
                finalSig.Add((double)c.Re);
            }

            //create the output file
            double absoluteMax = finalSig.Max();
            double divideNum = absoluteMax / 20000;

            using (BinaryWriter bb = new BinaryWriter(File.Open(output, FileMode.Create)))
            {
                for(int i = 0; i < finalFFT.Length; i++)
                {
                    double small = Math.Round(finalSig[i] / divideNum);
                    short outvar = System.Convert.ToInt16(small);
                    if (outvar != 0)
                    {
                        bb.Write(outvar);
                    }
                    
                }
            }
            //Ya Done Son!!!!!!!
            Console.WriteLine("YOYOYOYOYOYO");
        }

        public static Complex[] FFT(Complex[] a)
        {
            int n = a.Length;
            if(n == 1)
            {
                return a;
            }
            Complex w = new Complex(1, 0);
            Complex wn = new Complex(Math.Cos(2 * Math.PI / n), Math.Sin(2 * Math.PI / n));
            Complex[] cEven = new Complex[n / 2];
            Complex[] cOdd = new Complex[n / 2];
            int j = 0;
            for (int i = 0; i < n; i += 2)
            {
                cEven[j] = a[i];
                j++;
            }
            int f = 0;
            for (int i = 1; i < n; i += 2)
            {
                cOdd[f] = a[i];
                f++;
            }
            Complex[] AEven = FFT(cEven);
            Complex[] AOdd = FFT(cOdd);
            Complex[] A = new Complex[n];
            for (int k = 0; k <= (n/2) - 1; k++)
            {
                A[k] = AEven[k] + w * AOdd[k];
                A[k + (n / 2)] = AEven[k] - w * AOdd[k];
                w = w * wn;
            }
            return A;
        }
    }
}
