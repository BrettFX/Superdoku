package com.trietech.superdokuapi;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileOutputStream;
import java.io.FileReader;
import java.io.IOException;
import java.io.OutputStream;
import java.sql.Date;
import java.sql.Timestamp;
import java.text.SimpleDateFormat;
import java.util.Arrays;
import java.util.HashMap;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import javax.ws.rs.Consumes;
import javax.ws.rs.GET;
import javax.ws.rs.HeaderParam;
import javax.ws.rs.PUT;
import javax.ws.rs.Path;
import javax.ws.rs.Produces;
import javax.ws.rs.core.MediaType;
import javax.ws.rs.core.Response;

import org.apache.commons.io.FileUtils;
import org.apache.log4j.Level;
import org.apache.log4j.Logger;
import org.apache.log4j.xml.DOMConfigurator;

/**
 * @author Brett
 *
 */
@Path("/rest")
public class SuperdokuAPI {
	
	public static final String TEMP_DIR = "/tmp";
	public static final String SUPERDOKU_CORE_DIR = "/opt/Superdoku";
	
	public static final Logger LOGGER = Logger.getLogger(SuperdokuAPI.class);
	static {
		DOMConfigurator.configure(new File(SUPERDOKU_CORE_DIR + "/log4j.xml").getAbsolutePath());
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
	 * @param byte[] imagePayload the byte array representing the image that is sent to invoke this request
	 * 
	 * @return the json response representing the array of integers for the classified digits 
	 * 		   of the Sudoku puzzle image.
	 * */
	@PUT
	@Path("/recognize")
	@Consumes(MediaType.APPLICATION_OCTET_STREAM)
	@Produces(MediaType.APPLICATION_JSON)	
	public Response recognize(byte[] imagePayload, @HeaderParam("Content-Disposition") String contentDisposition) {
		// Create a status report object to contain the main response in addition to any errors
		HashMap<String, Object> statusReport = new HashMap<>();	
		
		LOGGER.info("Processing image recognition request...");
		LOGGER.info("|-- data length: " + imagePayload.length);
		
		if (imagePayload.length > 0 ) {
			// Create file timestamp
			Date date = new Date(System.currentTimeMillis());  
		    Timestamp ts=new Timestamp(date.getTime());
		    SimpleDateFormat formatter = new SimpleDateFormat("yyyyMMdd_HHmm_ss_SSS");  
			String fileTimestamp = formatter.format(ts);
			
			// Get file type from content disposition in request header (treat as jpg by default)
			Pattern pattern = Pattern.compile("(?<=filetype=\").*?(?=\")");
			Matcher fileTypeMatcher = pattern.matcher(contentDisposition);
			String fileType = fileTypeMatcher.find() ? 
											 contentDisposition.substring(fileTypeMatcher.start(), fileTypeMatcher.end())
											 : "jpg";
			
			LOGGER.info("File type: " + fileType);

			String basename = "superdoku-snap_" + fileTimestamp;
			String uploadFileLocation = TEMP_DIR + "/" + basename + "." + fileType;

		    // Save the uploaded image file
		    writeToFile(imagePayload, uploadFileLocation);
		    
		    // Invoke Python process to recognize sudoku image and parse grid
		    parseGrid(uploadFileLocation);
		    
		    // Read array from output file
		    String csvLine = "";
		    final String outpath = SUPERDOKU_CORE_DIR + "/" + basename + ".out";
		    File outfile = new File(outpath);
			try {
				csvLine = head(outfile, 1);
			} catch (IOException e) {
				LOGGER.error("IOException:", e);
			}
			
		    LOGGER.info("Classified Digits: " + csvLine);
		    
		    // Tokenize the puzzle string to a string array and then parse to an integer array
		    String[] tokens = csvLine.trim().split(","); // Be sure to trim to get rid of newline characters
		    int[] puzzle = Arrays.asList(tokens).stream().mapToInt(Integer::parseInt).toArray();
		    
		    // Put the parsed puzzle in the status report for processing on client side
		    statusReport.put("puzzle", puzzle);
		    
		    // Remove the snapped image
		    boolean deleted = FileUtils.deleteQuietly(new File(uploadFileLocation));
		    LOGGER.info(deleted ? "Deleted " + uploadFileLocation : "Could not delete " + uploadFileLocation);
		    
		    // Delete temporary snap outfile as well
		    deleted = FileUtils.deleteQuietly(outfile);
		    LOGGER.info(deleted ? "Deleted " + outpath : "Could not delete " + outpath);
		} else {
			statusReport.put("error", "Could not get request data to write file with.");
		}
		
		// Return the test Sudoku puzzle for now		
		return Response.ok()
				.entity(statusReport)
				.build();
	}
	
	@PUT
	@Path("uploadMessage")
    @Consumes(MediaType.APPLICATION_OCTET_STREAM)
	@Produces(MediaType.APPLICATION_JSON)
    public Response uploadMessage(byte[] payload, @HeaderParam("Content-Disposition") String contentDisposition){
		LOGGER.info("Content-Disposition: " + contentDisposition);
		
		String msg = new String(payload);
        LOGGER.info("Payload: " + msg);
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
	
	// Save uploaded file to new location
	private void writeToFile(byte[] payload, String path) {
		try { 
            // Initialize a pointer 
            // in file using OutputStream 
            OutputStream os = new FileOutputStream(new File(path)); 
  
            // Starts writing the bytes in it 
            os.write(payload); 
            LOGGER.info("Wrote payload data to " + path);
  
            // Close the file 
            os.flush();
            os.close(); 
        } catch (Exception e) { 
        	LOGGER.error("Exception: ", e);
        } 
	}
	
	/**
	 * Get the first n lines in a given file where n is an integer between 1 and infinity
	 * */
	private String head(File infile, int lines) throws IOException {
		// Default to 10 lines
		if (lines <= 0) {
			lines = 10;
		}
		
		BufferedReader br = new BufferedReader(new FileReader(infile));
		StringBuilder builder = new StringBuilder();
		
		String line;
		for (int i = 0; i < lines; i++) {
			line = br.readLine();
			if (line == null) {
				break;
			}
			
			builder.append(line + "\n");
		}
		
		br.close();		
		return builder.toString();
	}
	
	/**
	 * Invoke the main Python process with the run_extractor.sh shell script
	 * Need to call the shell script as a wrapper due to issues with running Python directly 
	 * */
	private void parseGrid(String imagePath) {
		
		String[] args = new String[] {
			"sh", 
			SUPERDOKU_CORE_DIR + "/run_extractor.sh",
			imagePath
		};
		
		ProcessBuilder pb = new ProcessBuilder();
		pb.command(args);
		pb.redirectErrorStream(true);
		
		try {
			Process process = pb.start();
            int exitCode = process.waitFor();
            LOGGER.info("Process finished with exit code " + exitCode + "\n");
		} catch (IOException e) {
			LOGGER.error("IOException: ", e);
		} catch (InterruptedException e) {
			LOGGER.error("InterruptedException: ", e);
		}
	}
}
