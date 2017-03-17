/**
* @author Brett Allen
* @created Mar 16, 2017, at 6:39:35 PM
* */

package application;

import javafx.fxml.FXML;
import javafx.scene.control.Button;
import javafx.scene.control.TextField;
import javafx.scene.input.KeyEvent;

public class SuperdokuController {
	
	private boolean isSolved;
	
	@FXML
	private Button btnSolve;
	
	@FXML
	private TextField txtCell;
	
	//An initialized 9x9 matrix
	private int[][] puzzle =
	{
			{0, 0, 0 ,  0, 0, 0,   0, 0, 0},
			{0, 0, 0,   0, 0, 0,   0, 0, 0},
			{0, 0, 0,   0, 0, 0,   0, 0, 0},
			
			{0, 0, 0,   0, 0, 0,   0, 0, 0},
			{0, 0, 0,   0, 0, 0,   0, 0, 0},
			{0, 0, 0,   0, 0, 0,   0, 0, 0},
			
			{0, 0, 0,   0, 0, 0,   0, 0, 0},
			{0, 0, 0,   0, 0, 0,   0, 0, 0},
			{0, 0, 0,   0, 0, 0,   0, 0, 0},
	};
	
	public SuperdokuController(){
		Superdoku.stage.setResizable(false);
	}
	
	/**
	 * This method acts as an action listener for the solve button
	 * Once btnSolve is clicked, the input cells are validated and parsed into a 
	 * 9x9 matrix to be used as input for the remainder of the solution process.
	 * */
	public void parseAndSolve(){
		long startTime, endTime;
		
		System.out.println("Here is the problem:\n");
		displayPuzzle();
		
		startTime = System.currentTimeMillis();
		
		//Begin the SuDoKu-solving algorithm at the beginning of the 9x9 matrix
		solvePuzzle(0, 0);	
		
		System.out.println("\nHere is the solution:\n");
		displayPuzzle();
		
		endTime = System.currentTimeMillis();
		System.out.println("Solution took " + (endTime - startTime) + " millisecond(s) to derive.");
	}
	
	/**
	 * 
	 * */
	public void getInput(KeyEvent event){
		//Get the cell id from the event source
		String cellID = event.getSource().toString().substring(16, 18);	
		
		//Get the contents of the cell in which the data was entered
		TextField cell = (TextField)event.getSource();
		
		validateTextField(cell);
		
		//System.out.println(cell.getText() + " was entered for row: " + cellID.charAt(0) + " column: " + cellID.charAt(1));
		
		//Insert the number from extracted from the GUI to the puzzle if it is not empty and it is valid
		if(!cell.getText().isEmpty()){
			int r = Character.getNumericValue(cellID.charAt(0));
			int c = Character.getNumericValue(cellID.charAt(1));
			
			puzzle[r][c] = Integer.parseInt(cell.getText());			
		}
	}
	
	/**
	 * 
	 * */
	private void validateTextField(TextField cell){
		for(char c : cell.getText().toCharArray()){
			if(Character.isAlphabetic(c) || c == '0'){
				cell.clear();
				return;
			}
		}
		
		int cellVal = Integer.parseInt(cell.getText());
		
		if(cellVal < 0 || cellVal > 9){
			cell.clear();
		}
	}
	
	/**
	 * Displays the SuDoKu puzzle to the console
	 * 
	 * @param p the two-dimensional 9x9 puzzle to be displayed
	 * */
	public void displayPuzzle(){
		//The character to determine formatting
		String d;
		
		for(int x = 0; x < puzzle.length; x++){			
			for(int y = 0; y < puzzle[x].length; y++){	
				//Print out a divider every third line
				d = (y % 3 == 0) ? "\t" : " ";
				System.out.print(d + puzzle[x][y]);				
			}	
			
			d = ((x + 1) % 3 == 0) ? "\n" : "";
			System.out.println(d);
		}
	}
	
	/**
	 * Driver method to solve the SuDoKu puzzle.
	 * Recursively determines the correct numbers for each cell
	 * 
	 * @param the two-dimensional 9x9 puzzle to be solved 
	 * @param row the current row within the 9x9 SuDoKu puzzle
	 * @param col the current column  within the 9x9 SuDoKu puzzle
	 * */
	public void solvePuzzle(int row, int col){
		
		if(col > puzzle[row].length - 1){
			col = 0;
			row++;
		}
		
		//Puzzle solved (we reached the last cell without any errors)
		if(row > puzzle.length - 1 || isSolved){
			isSolved = true;
			return;
		}		
		
		if(puzzle[row][col] == 0){
			//Try each number 1-9 until it satisfies all three rules
			for(int num = 1; num <= 9; num++){	
				if(followsRowRule(row, num) && followsColRule(col, num) && followsSquareRule(row, col, num)){
					puzzle[row][col] = num;
					solvePuzzle(row, col + 1);	
					
					if(isSolved) 
						return;	
					
					puzzle[row][col] = 0;
				}
			}		
		}else{
			//There was an error. We must go back and correct it
			solvePuzzle(row, col + 1);
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
	public boolean followsRowRule(int row, int num){
		for(int y = 0; y < puzzle[row].length; y++){
			//If a number in the row already exists then the number in question does not satisfy the row rule
			if(puzzle[row][y] == num){
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
	public boolean followsColRule(int col, int num){
		for(int x = 0; x < puzzle.length; x++){
			//If a number in the column already exists then the number in question does not satisfy the column rule
			if(puzzle[x][col] == num){
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
	public boolean followsSquareRule(int row, int col, int num){
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
					if(puzzle[x][y] == num){
						return false;
					}
				}
			}
		}
		
		return true;
	}
}