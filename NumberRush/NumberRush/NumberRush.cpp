// Ollie Robinson 2020
// Number Rush!
// Type the numbers in the correct order before they reach the left side of the screen, or it's game over!
// Heavily based on the OLC Tetris project.
// My first C++ "game" that I made by myself... excluding the modifications of an existing project of course,
// but that's acceptable, right?

#include <iostream>
#include <Windows.h>
#include <thread>
#include <vector>
#include <string>
using namespace std;

constexpr int _screenWidth = 80;
constexpr int _screenHeight = 30;

int _fieldWidth = 25;
int _fieldHeight = 8;

unsigned char* _field = nullptr;

int main()
{
    // Generate the console screen buffer
    // I'll be honest apart from the first 2 lines I have no idea what any of this means.
    wchar_t* screen = new wchar_t[_screenWidth * _screenHeight];
    for (int i = 0; i < _screenWidth * _screenHeight; i++) screen[i] = L' ';
    HANDLE hConsole = CreateConsoleScreenBuffer(GENERIC_READ | GENERIC_WRITE, 0, NULL, CONSOLE_TEXTMODE_BUFFER, NULL);
    SetConsoleActiveScreenBuffer(hConsole);
    DWORD dwBytesWritten = 0;

    bool _gameOver = false; // Is the game over yet? No? Damn.

    // Generate player field
    _field = new unsigned char[_fieldWidth * _fieldHeight];
    for (int x = 0; x < _fieldWidth; x++) {
        for (int y = 0; y < _fieldHeight; y++) {
            _field[y * _fieldWidth + x] = (x == 0 || y == 0 || x == _fieldWidth - 1 || y == _fieldHeight - 1) ? 11 : 0; // Create a border around the field.
        }
    }

    // Create the random seed.
    srand(time(NULL));
    int _currentNumber;
    int _currentNumberY;
    vector<int> _numbers; // The number vector, contains all the numbers in order.
    
    // The shouldDraw boolean tells the renderer to push all the
    // numbers on the screen one cell to the left. Really this
    // should be called "pushLeft" or something like that but oh well.
    bool shouldDraw = true;
    // Likewise, the shouldGenerateNumber boolean tells the generator
    // to generate a new number. It's randomised, and with easier
    // difficulties, is programmed to be false more than true, to make
    // the game easier.
    bool shouldGenerateNumber = true;

    int _ticksToMoveForward = 40; // The "speed" of the game. The lower this number, the faster the movement.
    int currentTick = 0; // Current game tick.

    // This boolean is structured from 0-9, allowing me to easily use it
    // to work out if the correct key has been pressed corresponding
    // to the correct number on screen.
    bool getKey[10]; 
    bool keyHold = false;

    int score = 0;

    while (!_gameOver) { // Main game loop
        // Timing ===========================
        this_thread::sleep_for(10ms);
        currentTick++;
        if (currentTick >= _ticksToMoveForward) {
            shouldDraw = true;
            currentTick = 0;
        }

        // Input ============================
        for (int k = 0; k < 10; k++)
            getKey[k] = (0x8000 & GetAsyncKeyState((unsigned char)("0123456789"[k]))) != 0;

        // Render ===========================
        for (int x = 0; x < _fieldWidth; x++) {
            for (int y = 0; y < _fieldHeight; y++) {
                screen[((y + 1) * _screenWidth) + (x + 5)] = L" 0123456789#"[_field[y * _fieldWidth + x]];
            }
        }

        if (shouldDraw) {
            // Create scrolling effect
            for (int x = 2; x < _fieldWidth - 1; x++) {
                for (int y = 1; y < _fieldHeight - 1; y++) {
                    if (_field[y * _fieldWidth + 1] != 0) _gameOver = true; // Checks to see if any section of the leftmost "pixel" isn't empty. If this is true, game over!
                    _field[y * _fieldWidth + (x - 1)] = _field[y * _fieldWidth + x]; // Shift each "pixel" over by 1.
                    _field[y * _fieldWidth + x] = 0; // And set the current "pixel" to blank.
                }
            }

            //shouldGenerateNumber = (rand() % 2 == 0) ? true : false; // Randomly chooses if a number should be generated or not.

            if (shouldGenerateNumber) {
                _currentNumber = rand() % 9 + 1;
                _currentNumberY = rand() % (_fieldHeight - 2) + 1; // Generate a random Y position.
                _numbers.push_back(_currentNumber);
                _field[_currentNumberY * _fieldWidth + _fieldWidth - 2] = _currentNumber + 1; // Draw it.
            }

            shouldDraw = false;
        }

        if (!_numbers.empty()) {
            if (getKey[_numbers.front()]) {
                if (!keyHold) {
                    keyHold = true;
                    score += 25;
                    if (score % 100 == 0)
                        if (_ticksToMoveForward > 0) _ticksToMoveForward -= 2; // This doesn't work properly.
                    // Is the following code hacky? Probably
                    // Do I care? Yes, but not right now.
                    for (int x = 2; x < _fieldWidth - 1; x++) {
                        for (int y = 1; y < _fieldHeight - 1; y++) {
                            if (_field[y * _fieldWidth + x] - 1 == _numbers.front()) { // Find the first case of the number in question, and set it blank.
                                _field[y * _fieldWidth + x] = 0;
                                goto ERASE; // Is this the correct use case for a goto?
                            }
                        }
                    }
                ERASE:
                    _numbers.erase(_numbers.begin());
                }
            }
            else keyHold = false;
        }

        swprintf_s(&screen[2 * _screenWidth + _fieldWidth + 6], 16, L"SCORE: %8d", score);

        WriteConsoleOutputCharacter(hConsole, screen, _screenWidth * _screenHeight, { 0, 0 }, &dwBytesWritten);
    }
    CloseHandle(hConsole);
    cout << "Game over! You scored " << score << " points!" << endl;
    system("pause");
    return 0;
}