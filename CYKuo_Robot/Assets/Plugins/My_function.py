# coding: utf-8

model_dir = '../x64/Release/model/model.pb'

#get_ipython().run_line_magic('env', 'KERAS_BACKEND = tensorflow')
import tensorflow as tf

from keras.models import Sequential, Model
from keras.layers import Dense, Activation,Input
from keras.optimizers import SGD, Adam

from keras.layers import concatenate, add
from keras.layers.core import Lambda
from keras import backend as K

import numpy as np
import os
import time
import sys

def Create_model():

	# Model
	x = Input(shape=(4,))

	## Function
	F_1 = Dense(90 , activation = "relu")

	F_2 = Dense(30 , activation = "tanh")
	F_3 = Dense(30 , activation = "softmax")
	F_4 = Dense(30 , activation = "relu")

	F_5 = Dense(50 , activation = "sigmoid")

	F_6 = Dense(10 , activation = "sigmoid")

	F_7 = Dense(4 , activation = "sigmoid")

	## hiden layer
	L_11 = F_1(x)

	L_21 = F_2(L_11)
	L_22 = F_3(L_11)
	L_23 = F_4(L_11)
	u1 = concatenate([L_21,L_22,L_23])

	L_31 = F_5(u1)
	L_12 = F_6(x)
	u2 = concatenate([L_31,L_12])

	y = F_7(u2)

	model = Model(x,y)
	model.compile(optimizer="adam",loss = "mean_squared_error",metrics = ['acc'])
	
	# Summary
	#print("model summary")
	#model.summary()
	return model


def show_summary(model):
	print("model summary")
	model.summary()


def Train_model(train_x,train_y,epochs): #train_x,train_y,epochs
	print("[INFO]Start Init!")

	model = Create_model()
	
	# Summary
	show_summary(model)

	# Start_train
	model.fit(train_x, train_y, epochs=epochs)
	test_loss, test_acc = model.evaluate(train_x, train_y)
	print('\nTest accuracy: {}'.format(test_acc))

	#Save_weights
	model.save_weights(model_dir)
	print("save model to: {}".format(model_dir))
	return model





def model_with_pretrained_weight(dir):
	print(dir)
	model = Create_model()
	model.load_weights(dir)
	model.summary()
	return model

def model_predict(model,predict_x):
	predict_x = np.asarray(predict_x)
	
	answer = model.predict(predict_x)

	print(answer)
	return answer.tolist()

def Print_list(data_x,data_y):
	print("data_x:")
	print(data_x)
	print("data_y:")
	print(data_y)
	#np.array()

# 預設資料
def default_data_x():
	train_x = np.array([[1,2,3,4],[5,6,7,8],[1,3,5,7],[2,4,6,8],[3,5,7,6],[4,1,2,3]])
	return train_x

def default_data_y():
	train_y = np.array([[0.1,0.2,0.3,0.4],[10/26,12/26,14/26,16/26],[ 2/22,6/22,10/22,14/22],[ 4/40, 8/40,12/40, 16/40], [ 6/42,10/42,14/42,12/42],[ 8/20, 2/20, 4/20, 6/20]])
	return train_y