using System;
using Nancy;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using Nancy.Responses;
using System.IO;

namespace ocrproject
{
	public class Main : NancyModule
	{

		public Main ()
		{
			Get ["/"] = parameters => {
				return new RedirectResponse ("/ocr");
			};

			Get ["/ocr"] = parameters => {
				return View ["website.html"];
			};

			Put ["/ocr"] = this.PhotoToText;

			Post ["/ocr"] = this.PhotoToText;
		}

		public Response PhotoToText (dynamic parameters)
		{
			var fileToOCR = this.Request.Files.FirstOrDefault ();

			//TODO: If we don't have a file, return an error.

			// 1. Get the temporary directory for your system/app
			//    or define one
			// 2. Write the fileToOCR.Value stream to a file in that directory
			//    with a unique name. I.e. a GUID/UUID or something like fileToOCR.Name + "-" + timestamp
			// 3. Use that filename in the .Arguments variable below in place of our static filename

			string tempFileName = Path.GetTempFileName ();

			using (var fileStream = File.Open (tempFileName, FileMode.OpenOrCreate)) {
				fileToOCR.Value.Seek (0, SeekOrigin.Begin);
				fileToOCR.Value.CopyTo (fileStream);
			}

			var tesseractProcess = new Process ();
			tesseractProcess.StartInfo.UseShellExecute = false;
			tesseractProcess.StartInfo.RedirectStandardOutput = true;
			tesseractProcess.StartInfo.RedirectStandardError = true;
			tesseractProcess.StartInfo.FileName = "/usr/local/bin/tesseract";
			tesseractProcess.StartInfo.Arguments = String.Format ("{0} stdout", tempFileName);

			String output = "could not read";

			try {
				tesseractProcess.Start ();
				tesseractProcess.WaitForExit ();

				if (tesseractProcess.ExitCode == 0) {
					output = tesseractProcess.StandardOutput.ReadToEnd ();
				} else {
					output += ". Reason: " + tesseractProcess.StandardError.ReadToEnd ();
				}						
			} catch (Exception ex) {
				output += ". Reason: " + ex.Message;
			}				

			try {
				File.Delete (tempFileName);
			} catch {
			}



			return output;
		}
			
			
	}


}

