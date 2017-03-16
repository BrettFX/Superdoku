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
		
		System.out.println("Here is the problem:");
		displayPuzzle(p);
		
		startTime = System.currentTimeMillis();
		
		//Begin the SuDoKu-solving algorithm at the beginning of the 9x9 matrix
		solvePuzzle(p, 0, 0);	
		
		System.out.println("\nHere is the solution:");
		displayPuzzle(p);
		
		endTime = System.currentTimeMillis();
		System.out.println("Solution took " + (endTime - startTime) + " milliseconds to derive.");		
		
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
	 * Driver method to solve the SuDoKu puzzle.
	 * Recursively determines the correct numbers for each cell
	 * 
	 * @param the two-dimensional 9x9 puzzle to be solved 
	 * @param row the current row within the 9x9 SuDoKu puzzle
	 * @param col the current column  within the 9x9 SuDoKu puzzle
	 * */
	public static void solvePuzzle(int p[][], int row, int col){
		
		if(col >= p[row].length){
			col = 0;
			row++;
		}
		
		//Puzzle solved (we reached the last cell without any errors)
		if(row >= p.length){
			return;
		}		
		
		//Try each number 1-9 until it satisfies all three rules
		for(int num = 1; num <= 9; num++){
			if(p[row][col] == 0){
				if(followsRowRule(p, row, num) && followsColRule(p, col, num) && followsSquareRule(p, row, col, num)){
					p[row][col] = num;
					solvePuzzle(p, row, col + 1);
					p[row][col] = 0;
				}else{
					//There was an error. We must go back and correct it
					solvePuzzle(p, row, col + 1);
					return;
				}
			}		
			
			
		}			
	}
	
	/**
	 * Determines if the number to be inserted has not already been used in the current row
	 * 
	 * @param p the two-dimensional 9x9 puzzle
	 * @param row the row to be determined if the number to be inserted is valid
	 * @param num the number to determine is valid
	 * @return whether the number to be inserted is valid or not
	 * */
	public static boolean followsRowRule(int p[][], int row, int num){
		for(int y = 0; y < p[row].length; y++){
			//If a number in the row already exists then the number in question does not satisfy the row rule
			if(p[row][y] == num){
				return false;
			}
		}
		return true;
	}
	
	/**
	 * Determines if the number to be inserted has not already been used in the current column
	 * 
	 * @param p the two-dimensional 9x9 puzzle
	 * @param col the column of the puzzle to be determined if valid
	 * @param num the number to determine is valid
	 * @return whether the number to be inserted is valid or not
	 * */
	public static boolean followsColRule(int p[][], int col, int num){
		for(int x = 0; x < p.length; x++){
			//If a number in the column already exists then the number in question does not satisfy the column rule
			if(p[x][col] == num){
				return false;
			}
		}
		
		return true;
	}
	
	/**
	 * Determines if the number to be inserted has not already been used in the 3x3 subspace
	 * that the current cell lives in
	 * 
	 * @param p the two-dimensional 9x9 puzzle
	 * @param row the row in the 3x3 subspace of the puzzle to be determined if valid
	 * @param col the column in the 3x3 subspace of the puzzle to be determined if valid
	 * @param num the number to determine is valid
	 * @return whether the number to be inserted is valid within the 3x3 subspace or not
	 * */
	public static boolean followsSquareRule(int p[][], int row, int col, int num){
		int xStart, yStart;
		
		if(row >= 6){
			xStart = 6;
		}else if(row >= 3){
			xStart = 3;
		}else{
			xStart = 0;
		}
		
		if(col >= 6){
			yStart = 6;
		}else if(col >= 3){
			yStart = 3;
		}else{
			yStart = 0;
		}
		
		for(int x = xStart; x < xStart + 3; x++){
			for(int y = yStart; y < yStart + 3; y++){				
				if(x!= row && y!= col){
					if(p[x][y] == num){
						return false;
					}
				}
			}
		}
		
		return true;
	}
}
