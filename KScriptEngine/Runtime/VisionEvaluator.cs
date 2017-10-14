using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//using asprise_ocr_api;

namespace KScript
{
    public class VisionEvaluator
    {
        //[DllImport("AspriseOCR.dll", EntryPoint = "OCR", CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr OCR(string file, int type);
        //[DllImport("AspriseOCR.dll", EntryPoint = "OCRpart", CallingConvention = CallingConvention.Cdecl)]
        //static extern IntPtr OCRpart(string file, int type, int startX, int startY, int width, int height);
        //[DllImport("AspriseOCR.dll", EntryPoint = "OCRBarCodes", CallingConvention = CallingConvention.Cdecl)]
        //static extern IntPtr OCRBarCodes(string file, int type);
        //[DllImport("AspriseOCR.dll", EntryPoint = "OCRpartBarCodes", CallingConvention = CallingConvention.Cdecl)]
        //static extern IntPtr OCRpartBarCodes(string file, int type, int startX, int startY, int width, int height);

        //public object Execute(string imgPath)
        //{
        //    //string script = Marshal.PtrToStringAnsi(OCRpart(imgPath, -1, 0, 0, 1000, 1000));
        //    AspriseOCR.SetUp();
        //    AspriseOCR ocr = new AspriseOCR();
        //    ocr.StartEngine("eng", AspriseOCR.SPEED_FASTEST);
        //    string script = ocr.Recognize(@"F:\Backup\DeskFiles\1234.png", -1, -1, -1, -1, -1
        //        , AspriseOCR.RECOGNIZE_TYPE_TEXT, AspriseOCR.OUTPUT_FORMAT_PLAINTEXT);
        //    //Evaluator eval = new Evaluator(script);
        //    //eval.Execute(new NestedEnv());
        //    ocr.StopEngine();
        //    //return eval.GetVariable("a");
        //    return script;
        //}
    }
}
