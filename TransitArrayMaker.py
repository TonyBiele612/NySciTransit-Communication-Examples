import pygame as pg
import random
import TransitSocketClient as tsc
#import TransitSocketServer as tss

SCREEN_WIDTH = 500
SCREEN_HEIGHT = 500
FRAMERATE = 30

MAGENTA = (255, 0, 255)
BLACK = (0, 0, 0)

size = [SCREEN_WIDTH, SCREEN_HEIGHT]
screen = pg.display.set_mode(size)
done = False;
all_point_list = []

tsc.CreateSocket()

def AddPoint():   
    newPoints = [pg.mouse.get_pos()[0] / SCREEN_WIDTH,
                 pg.mouse.get_pos()[1] / SCREEN_HEIGHT,
                 random.randint(0,10)]
             
    all_point_list.append(newPoints)
    print(all_point_list)
    print("List Count :", len(all_point_list))

def RemoveRandomPoint():
    if len(all_point_list) == 0:
        return
    all_point_list.pop(random.randint(0, len(all_point_list) -1))
    print(all_point_list)
    print("List Count :", len(all_point_list))

while not done:
    for event in pg.event.get():
        if event.type == pg.QUIT:
            done = True
        elif event.type == pg.KEYDOWN:
            if event.key == pg.K_ESCAPE:
                done = True
        elif event.type == pg.MOUSEBUTTONDOWN:
            if event.button == 1:
                AddPoint()
                tsc.SendData(all_point_list)
            elif event.button == 3:
                RemoveRandomPoint()
                tsc.SendData(all_point_list)

    screen.fill(BLACK)
    for point in all_point_list:
        pg.draw.circle(screen, MAGENTA, [point[0] * SCREEN_WIDTH,point[1] * SCREEN_HEIGHT], 5)
        
    pg.display.flip()

pg.quit()







