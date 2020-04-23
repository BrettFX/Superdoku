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

import com.aquafx_project.AquaFx;

import javafx.application.Application;
import javafx.fxml.FXMLLoader;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.stage.Stage;

public class Superdoku extends Application {
	
	@Override
	public void start(Stage primaryStage) {
		
		try {			
			FXMLLoader loader = new FXMLLoader(getClass().getResource("resources/Superdoku.fxml"));
			Parent root = (Parent)loader.load();
			SuperdokuController controller = (SuperdokuController)loader.getController();
			controller.initialize(primaryStage);			
			Scene scene = new Scene(root);			
			primaryStage.setScene(scene);			
			primaryStage.show();
			AquaFx.style();
		} catch(Exception e) {
			e.printStackTrace();
		}
	}
	
	public static void main(String[] args) {
		launch(args);
	}
}
