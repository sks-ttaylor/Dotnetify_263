import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';

import { ChatRoom } from './components/ChatRoom';

//import './custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <Layout>
        <Route exact path='/' component={Home} />
        <Route path='/counter' component={Counter} />
        <Route path='/fetch-data' component={FetchData} />
        <Route path='/chat/InstA' render={() =><ChatRoom InstanceId="InstA" />} />
        <Route path='/chat/InstB' render={() =><ChatRoom InstanceId="InstB" />} />
      </Layout>
    );
  }
}
