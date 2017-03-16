/**
 * @author Brett Allen
 * @created 03/06/17 at 3:48 PM
 * 
 * Superdoku is a a tool to be used to solve sudoku puzzles of varying difficulty.
 * This software application features a visually aesthetic graphical user interface
 * that allows the user to choose amongst input methods. The user may choose to enter
 * the contents of an unsolved sudoku puzzle, supply a copy of the data in a command line-like
 * entry, or upload an image/snapshot of a puzzle to be parsed by the software
 * */

package application;

import javafx.application.Application;
import javafx.stage.Stage;
import javafx.scene.Scene;
import javafx.scene.layout.BorderPane;

public class Superdoku extends Application {
	
	public static boolean isSolved = false;
	
	public static final int[][] PUZZLE1 =
	{
			{0, 0, 3 ,  0, 0, 0,   0, 0, 4},
			{6, 0, 0,   0, 0, 0,   0, 9, 0},
			{0, 1, 0,   8, 0, 0,   0, 0, 0},
			
			{0, 0, 4,   0, 0, 9,   0, 3, 0},
			{0, 7, 0,   0, 0, 0,   0, 0, 8},
			{5, 0, 0,   2, 0, 0,   6, 0, 0},
			
			{0, 8, 0,   0, 0, 4,   0, 7, 0},
			{0, 0, 9,   0, 6, 0,   0, 0, 0},
			{2, 0, 0,   3, 0, 0,   5, 0, 0},
	};	
	
	@Override
	public void start(Stage primaryStage) {
		try {
			BorderPane root = new BorderPane();
			Scene scene = new Scene(root,400,400);
			scene.getStylesheets().add(getClass().getResource("application.css").toExternalForm());
			primaryStage.setScene(scene);
			primaryStage.show();
		} catch(Exception e) {
			e.printStackTrace();
		}
	}
	
	public static void main(String[] args) {
		int[][] p = PUZZLE1;
		long startTime, endTime;
		
		SuperdokuController test = new SuperdokuController();
		
		System.out.println("Here is the problem:\n");
		test.displayPuzzle(p);
		
		startTime = System.currentTimeMillis();
		
		//Begin the SuDoKu-solving algorithm at the beginning of the 9x9 matrix
		test.solvePuzzle(p, 0, 0);	
		
		System.out.println("\nHere is the solution:\n");
		test.displayPuzzle(p);
		
		endTime = System.currentTimeMillis();
		System.out.println("Solution took " + (endTime - startTime) + " millisecond(s) to derive.");		
		
		//launch(args);
	}
}
