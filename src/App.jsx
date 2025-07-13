import { BrowserRouter, Routes, Route } from 'react-router-dom';
import '../src/globalstyles/App.css'

import Login from './pages/login/login'
import CreateAccount from './pages/CreateAccount/CreateAccount'

import Story from './pages/story/Story'

function App() {

  return (

    <BrowserRouter>
      <Routes>

        <Route path='/Login' element={<Login/>}/>
        <Route path='/CreateAccount' element={<CreateAccount/>}/>

        <Route path='/' element={<Story/>}/>

      </Routes>
    </BrowserRouter>

  )
}

export default App;