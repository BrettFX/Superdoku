/**
* @author Brett Allen
* @created Mar 16, 2017, at 6:39:35 PM
* */

package application;

public class SuperdokuController {
	
	private boolean isSolved;
	
	/**
	 * Displays the SuDoKu puzzle to the console
	 * 
	 * @param p the two-dimensional 9x9 puzzle to be displayed
	 * */
	public void displayPuzzle(int p[][]){
		//The character to determine formatting
		String d;
		
		for(int x = 0; x < p.length; x++){			
			for(int y = 0; y < p[x].length; y++){	
				//Print out a divider every third line
				d = (y % 3 == 0) ? "\t" : " ";
				System.out.print(d + p[x][y]);				
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
	public void solvePuzzle(int p[][], int row, int col){
		
		if(col > p[row].length - 1){
			col = 0;
			row++;
		}
		
		//Puzzle solved (we reached the last cell without any errors)
		if(row > p.length - 1 || isSolved){
			isSolved = true;
			return;
		}		
		
		if(p[row][col] == 0){
			//Try each number 1-9 until it satisfies all three rules
			for(int num = 1; num <= 9; num++){	
				if(followsRowRule(p, row, num) && followsColRule(p, col, num) && followsSquareRule(p, row, col, num)){
					p[row][col] = num;
					solvePuzzle(p, row, col + 1);	
					
					if(isSolved) 
						return;	
					
					p[row][col] = 0;
				}
			}		
		}else{
			//There was an error. We must go back and correct it
			solvePuzzle(p, row, col + 1);
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
	public boolean followsRowRule(int p[][], int row, int num){
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
	public boolean followsColRule(int p[][], int col, int num){
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
	public boolean followsSquareRule(int p[][], int row, int col, int num){
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
