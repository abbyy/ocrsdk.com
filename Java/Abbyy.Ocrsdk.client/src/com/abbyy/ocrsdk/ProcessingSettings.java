package com.abbyy.ocrsdk;

public class ProcessingSettings {

	public String asUrlParams() {
		return String.format("language=%s&exportFormat=%s", language,
				outputFormat);
	}

	public enum OutputFormat {
		txt, rtf, docx, xlsx, pptx, pdfSearchable, pdfTextAndImages, xml
	}

	public void setOutputFormat(OutputFormat format) {
		outputFormat = format;
	}

	public OutputFormat getOutputFormat() {
		return outputFormat;
	}

	/*
	 * Set recognition language. You can set any language listed at
	 * http://ocrsdk.com/documentation/specifications/recognition-languages/ or
	 * set comma-separated combination of them.
	 * 
	 * Examples: English English,ChinesePRC English,French,German
	 */
	public void setLanguage(String newLanguage) {
		language = newLanguage;
	}

	public String getLanguage() {
		return language;
	}

	private String language = "English";
	private OutputFormat outputFormat = OutputFormat.pdfSearchable;
}
