package com.trietech.superdokuapi;

import java.io.File;

import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.PUT;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;

import org.apache.log4j.Level;
import org.apache.log4j.Logger;
import org.apache.log4j.xml.DOMConfigurator;

/**
 * @author Brett
 *
 */
@Path("/rest")
public class SuperdokuAPI {
	
	public static final Logger LOGGER = Logger.getLogger(SuperdokuAPI.class);
	static {
		DOMConfigurator.configure(new File("/opt/Superdoku/log4j.xml").getAbsolutePath());
		LOGGER.setLevel(Level.INFO);
	}
	
	public static final int[] TEST_PUZZLE = {
		5, 3, 0,     0, 7, 0,    0, 0, 0,
        6, 0, 0,     1, 9, 5,    0, 0, 0,
        0, 9, 8,     0, 0, 0,    0, 6, 0,

        8, 0, 0,     0, 6, 0,    0, 0, 3,
        4, 0, 0,     8, 0, 3,    0, 0, 1,
        7, 0, 0,     0, 2, 0,    0, 0, 6,

        0, 6, 0,     0, 0, 0,    2, 8, 0,
        0, 0, 0,     4, 1, 9,    0, 0, 5,
        0, 0, 0,     0, 8, 0,    0, 7, 9			
	};
	
	/**
	 * Invoke the image recognition task of the SudokuExtractor library.
	 * Expects an image as the request body in an application/octet-stream format.
	 * 
	 * @param byte[] imageData the byte array representing the image that is sent to invoke this request
	 * 
	 * @return the json response representing the array of integers for the classified digits 
	 * 		   of the Sudoku puzzle image.
	 * */
	@PUT
	@Path("/recognize")
	@Consumes(MediaType.APPLICATION_OCTET_STREAM)
	@Produces(MediaType.APPLICATION_JSON)	
	public Response recognize(byte[] imageData) {
		LOGGER.info("Processing image recognition request...");
		LOGGER.info("|-- data length: " + imageData.length);
		
		// Do Processing here
		
		// Return the test Sudoku puzzle for now		
		return Response.ok()
				.entity(TEST_PUZZLE)
				.build();
	}
	
	@GET
	@Path("/ping")
	@Produces(MediaType.APPLICATION_JSON)
	public Response statusCheck() { 
		LOGGER.info("Received ping request.");
		return Response.ok()
				.entity("pong")
				.build(); 
	}
}
