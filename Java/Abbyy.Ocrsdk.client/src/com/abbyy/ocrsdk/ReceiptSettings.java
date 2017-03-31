package com.abbyy.ocrsdk;

/**
 * Receipt processing settings.
 * 
 * For all possible settings see
 * http://ocrsdk.com/documentation/apireference/processReceipt/
 */

public class ReceiptSettings {
	
	public String asUrlParams() {
		// For all possible parameters, see documentation at
		// http://ocrsdk.com/documentation/apireference/processReceipt/
		return String.format("country=%s", receiptCountry);
	}
	
	/*
	 * Set country where receipt was printed. You can set any country listed at
	 * http://ocrsdk.com/documentation/apireference/processReceipt/ or
	 * set comma-separated combination of them.
	 * 
	 * Examples: Usa Usa,Spain
	 */
	public void setReceiptCountry(String newCountry) {
		receiptCountry = newCountry;
	}

	public String getReceiptCountry() {
		return receiptCountry;
	}

	private String receiptCountry = "Usa";
}
