package microsoft.hpc.scheduler;

public class Base64 {
	private final static String _base64Str =
			"ABCDEFGHIJKLMNOPQRSTUVWXYZ" + 
			"abcdefghijklmnopqrstuvwxyz" +
			"0123456789+/";
	
	/**
	 * Encodes a byte[] into base64 string
	 * @param inputBytes
	 * @return base64 encoded string
	 */
	public static String encode(byte[] inputBytes)
	{
		String encodedString = "";
		
		Boolean padding = false;
		Boolean doublePadding = false;
		
		int byteLen = inputBytes.length;
		
		int i = 0;
		while(i < byteLen)
		{
			byte b1 = inputBytes[i++];
			byte b2 = 0;
			byte b3 = 0;
			
			//if we are at the end of the byteArray
			if(i >= byteLen)
			{
				doublePadding = true;
			}
			else
			{
				b2 = inputBytes[i++];
				if(i >= byteLen)
				{
					padding = true;
				}
				else
				{
					b3 = inputBytes[i++];
				}
			}
			
			byte encodedb1 = (byte)(b1 >> 2);
			byte encodedb2 = (byte)(((b1 & 0x3) << 4) | (b2 >> 4));
			byte encodedb3 = (byte)(((b2 & 0xf) << 2) | (b3 >> 6));
			byte encodedb4 = (byte)(b3 & 0x3f);
			
			encodedString += _base64Str.charAt(encodedb1);
			encodedString += _base64Str.charAt(encodedb2);
			
			if(doublePadding)
			{
				encodedString += "==";
			}
			else
			{
				encodedString += _base64Str.charAt(encodedb3);
				if(padding)
				{
					encodedString += "=";
				}
				else
				{
					encodedString += _base64Str.charAt(encodedb4);
				}
			}
		}
		
		return encodedString;
	}
	
	/**
	 * Encodes a String into a base64 string
	 * @param inputString
	 * @return base64 encoded string
	 */
	public static String encode(String inputString)
	{
		return encode(inputString.getBytes());
	}
}
