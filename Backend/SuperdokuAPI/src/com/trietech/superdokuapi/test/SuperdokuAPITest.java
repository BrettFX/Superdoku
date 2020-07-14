package com.trietech.superdokuapi.test;

import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.IOException;

import org.apache.commons.io.IOUtils;
import org.junit.Assert;
import org.junit.Test;

import io.restassured.RestAssured;
import io.restassured.response.Response;
import io.restassured.specification.RequestSpecification;

public class SuperdokuAPITest {
	
	private static final String BASE_URL = "http://192.168.0.238:8080/SuperdokuAPI/rest";
	
	@Test
	public void testUploadMessage() {
		byte[] payload = "Test message payload".getBytes();
		
		RestAssured.baseURI = BASE_URL;
		RequestSpecification request = RestAssured.given();
		
		final String contentType = "plain-text";
		request.headers("Content-Disposition", "attachment; filetype=\"" + contentType + "\"");
		request.body(payload);
		Response response = request.put("/uploadMessage");

		int statusCode = response.getStatusCode();
		System.out.println(response.asString());
		Assert.assertEquals(statusCode, 200); 
	}
	
	@Test
	public void testRecognizeWithRawText() {
		byte[] imagePayload = "Test image payload".getBytes();
		
		RestAssured.baseURI = BASE_URL;
		RequestSpecification request = RestAssured.given();
		
		final String contentType = "txt";
		request.headers("Content-Disposition", "attachment; filetype=\"" + contentType + "\"");
		request.body(imagePayload);
		Response response = request.put("/recognize");

		int statusCode = response.getStatusCode();
		System.out.println(response.asString());
		Assert.assertEquals(statusCode, 200);
	}
	
	@Test
	public void testRecognizeWithSudokuImage() {
		try {
			byte[] imagePayload = IOUtils.toByteArray(new FileInputStream(new File("res/phone_snapped_sudoku_test.jpg")));
			RestAssured.baseURI = BASE_URL;
			RequestSpecification request = RestAssured.given();
			
			final String contentType = "jpg";
			request.headers("Content-Disposition", "attachment; filetype=\"" + contentType + "\"");
			request.body(imagePayload);
			Response response = request.put("/recognize");

			int statusCode = response.getStatusCode();
			System.out.println("Unit Test Response: " + response.asString());
			Assert.assertEquals(statusCode, 200);
		} catch (FileNotFoundException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		}
	}
}
