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
	
	public static final int[][] PUZZLE1 = 
	{
			{0, 4, 0 ,  0, 0, 0,   0, 6, 0},
			{7, 0, 6,   2, 0, 0,   0, 0, 0},
			{0, 0, 0,   4, 0, 8,   0, 3, 0},
			
			{0, 0, 2,   0, 0, 0,   5, 9, 0},
			{0, 0, 4,   0, 3, 0,   7, 0, 0},
			{0, 1, 5,   0, 0, 0,   4, 0, 0},
			
			{0, 9, 0,   1, 0, 5,   0, 8, 0},
			{0, 0, 0,   0, 0, 9,   2, 0, 0},
			{0, 2, 0,   0, 0, 0,   0, 7, 0},
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
		
		displayPuzzle(p);
		
		startTime = System.currentTimeMillis();
		
				
		
		endTime = System.currentTimeMillis();
		System.out.println("Solved puzzle in " + (endTime - startTime) + " milliseconds.");		
		
		//launch(args);
	}
	
	/**
	 * Displays the SuDoKu puzzle to the console
	 * 
	 * @param p the two-dimensional 9x9 puzzle to be displayed
	 * */
	public static void displayPuzzle(int p[][]){
		System.out.println("+-----------------+");
		
		for(int x = 0; x < p.length; x++){
			
			if(x!= 0 && x % 3 == 0){
				System.out.println("|-----+-----+-----|");
			}
			
			for(int y = 0; y < p[x].length; y++){	
				//Print out a divider every third line
				String d = (y % 3 == 0) ? "|" : " ";
				System.out.print(d + p[x][y]);				
			}
			
			System.out.println("|");			
		}
		
		System.out.println("+-----------------+");
	}
	
	/**
	 * Driver method to solve the SuDoKu puzzle
	 * 
	 * @param the two-dimensional 9x9 puzzle to be solved 
	 * */
	public static void solvePuzzle(int p[][]){
		for(int x = 0; x < p.length; x++){
			for(int y = 0; y < p[x].length; y++){
				if(followsRowRule(p, x) && followsColRule(p, y) && followsSquareRule(p, x, y)){
					
				}
			}
		}
	}
	
	/**
	 * Determines if the number to be inserted has not already been used in the current row
	 * 
	 * @param p the two-dimensional 9x9 puzzle
	 * @param row the row to be determined if the number to be inserted is valid
	 * @return whether the number to be inserted is valid or not
	 * */
	public static boolean followsRowRule(int p[][], int row){
		return true;
	}
	
	/**
	 * Determines if the number to be inserted has not already been used in the current column
	 * 
	 * @param p the two-dimensional 9x9 puzzle
	 * @param col the column of the puzzle to be determined if valid
	 * @return whether the number to be inserted is valid or not
	 * */
	public static boolean followsColRule(int p[][], int col){
		return true;
	}
	
	/**
	 * Determines if the number to be inserted has not already been used in the current column
	 * 
	 * @param p the two-dimensional 9x9 puzzle
	 * @param row the row in the 3x3 subspace of the puzzle to be determined if valid
	 * @param col the column in the 3x3 subspace of the puzzle to be determined if valid
	 * @return whether the number to be inserted is valid within the 3x3 subspace or not
	 * */
	public static boolean followsSquareRule(int p[][], int row, int col){
		return true;
	}
}
