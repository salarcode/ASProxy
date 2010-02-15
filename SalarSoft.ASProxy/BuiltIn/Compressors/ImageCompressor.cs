using System;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing;
using System.Reflection;

namespace SalarSoft.ASProxy.BuiltIn
{
	public static class ImageCompressor
	{
		/// <summary>
		/// Compresses images using configurations
		/// </summary>
		public static MemoryStream CompressImage(Stream imgStream)
		{
			using (Image img = Image.FromStream(imgStream))
			{
				ImageCodecInfo codecInfo;
				EncoderParameters encParam = new EncoderParameters(1);

				// Quality rate
				encParam.Param[0] = new EncoderParameter(
					Encoder.Quality,
					Configurations.ImageCompressor.Quality);

				// trying to find original image codec
				codecInfo = FindImageCodecInfo(img);

				// if nothing found, try to use own codec
				if (codecInfo == null)
				{

					// is 32bit alpha?
					if (Image.IsAlphaPixelFormat(img.PixelFormat))
					{
						// is animated
						if (IsImageAnimated(img))
						{
							// GIF
							// TODO: APNG in not implemented yet
							codecInfo = GetEncoderInfo("image/gif");
						}
						else
						{
							// PNG
							codecInfo = GetEncoderInfo("image/png");
						}
					}
					else
					{
						// This is default codec for any unknown format
						// JPEG
						codecInfo = GetEncoderInfo("image/jpeg");
					}
				}
				else
				{
					// if the native codec is PNG format
					if (codecInfo.MimeType.Equals("image/png", StringComparison.CurrentCultureIgnoreCase))
					{
						// if it is not 32bit
						if (Image.IsAlphaPixelFormat(img.PixelFormat) == false)
						{
							// if the image is not 32bit alpha so use JPEG codec
							codecInfo = GetEncoderInfo("image/jpeg");
						}
					}
				}


				// save to result
				MemoryStream result = new MemoryStream();
				img.Save(result, codecInfo, encParam);

				return result;
			}
		}

		/// <summary>
		/// Returns the image codec with the given mime type
		/// </summary>
		private static ImageCodecInfo GetEncoderInfo(string mimeType)
		{
			// Get image codecs for all image formats
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

			// Find the correct image codec
			for (int i = 0; i < codecs.Length; i++)
				if (codecs[i].MimeType == mimeType)
					return codecs[i];
			return null;
		}

		/// <summary>
		/// Warning, dirty hack! The FindEncoder is not public
		/// This method returns native used image codec.
		/// Why this is not public?
		/// </summary>
		private static MethodInfo _imageFormatFindEncoder = typeof(ImageFormat).GetMethod("FindEncoder", BindingFlags.Instance | BindingFlags.NonPublic);

		/// <summary>
		/// Warning, dirty hack! Image codec detector for Mono
		/// </summary>
		private static MethodInfo _monoImageFormatFindEncoder = typeof(Image).GetMethod("findEncoderForFormat", BindingFlags.Instance | BindingFlags.NonPublic);

		/// <summary>
		/// Returns image format's native image codec using a dirty hack
		/// </summary>
		/// <returns>if something found returns ImageCodecInfo, otherwise returns null</returns>
		private static ImageCodecInfo FindImageCodecInfo(Image image)
		{
			if (_imageFormatFindEncoder != null)
				return (ImageCodecInfo)_imageFormatFindEncoder.Invoke(image.RawFormat, null);
			else if(_monoImageFormatFindEncoder!=null)
				return (ImageCodecInfo)_monoImageFormatFindEncoder.Invoke(image, new object[] { image.RawFormat });
			return null;
		}

		/// <summary>
		/// Checks if it is animation
		/// </summary>
		private static bool IsImageAnimated(Image image)
		{
			try
			{
				return image.GetFrameCount(FrameDimension.Time) > 1;
			}
			catch
			{
				return false;
			}
		}
	}
}

