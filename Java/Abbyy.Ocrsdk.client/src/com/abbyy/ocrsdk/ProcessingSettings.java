package com.abbyy.ocrsdk;

public class ProcessingSettings {
	public String AsUrlParams()
	{
		return "language=English&exportFormat="+outputFormat.toString();
	}
	
	public enum OutputFormat {
		txt, rtf, docx, xlsx, pptx, pdfSearchable, pdfTextAndImages, xml
	}
	
	public void setOutputFormat( OutputFormat format ) 
	{
		outputFormat = format;
	}
	
	public OutputFormat getOutputFormat() 
	{
		return outputFormat;
	}
	
	private OutputFormat outputFormat = OutputFormat.pdfSearchable;
}
