/**
 * 
 */
package com.abbyy.ocrsdk;

/**
 * Business card processing settings.
 * 
 * For all possible settings see
 * http://ocrsdk.com/documentation/apireference/processBusinessCard/
 */
public class BusCardSettings {

	public String asUrlParams() {
		// For all possible parameters, see documentation at
		// http://ocrsdk.com/documentation/apireference/processTextField/
		return String.format("language=%s", language);
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
}
